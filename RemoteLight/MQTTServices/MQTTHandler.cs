using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using RemoteLight.Data;
using RemoteLight.Models;

namespace RemoteLight.MQTTServices
{
    public class MQTTHandler
    {
        private readonly IMqttClient mqttClient;
        public string BrokerIP { get; }
        private readonly int brokerPort;
        private readonly string connectionString;
        private ApplicationDbContext? _context;

        private readonly string recieveTopic;
        private readonly string responseTopic;

        public MQTTHandler(
            string connectionString,
            string brokerIP,
            int brokerPort,
            string recieveTopic,
            string responseTopic)
        {
            this.connectionString = connectionString;
            BrokerIP = brokerIP;
            this.brokerPort = brokerPort;
            this.recieveTopic = recieveTopic;
            this.responseTopic = responseTopic;

            mqttClient = new MqttFactory().CreateMqttClient();

            InitMqttClientConnection();
        }

        ~MQTTHandler()
        {
            Disconnect().Wait();
        }

        private async void InitMqttClientConnection()
        {
            mqttClient.ApplicationMessageReceivedAsync += (e) =>
            {
                RecieveMessage(e);
                return Task.CompletedTask;
            };

            try
            {
                await Connect();
                await Subscribe();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to broker {this.BrokerIP} on port {this.brokerPort}: {ex.Message}");
            }
        }

        public async Task Connect()
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(BrokerIP, brokerPort)
                .WithCleanSession()
                .Build();

            await mqttClient.ConnectAsync(options, CancellationToken.None);
        }

        public async Task Disconnect()
        {
            await mqttClient.DisconnectAsync();
        }

        public async Task Subscribe()
        {
            if (!mqttClient.IsConnected)
                await Connect();

            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(recieveTopic)
                .Build();
            await mqttClient.SubscribeAsync(topicFilter);
        }

        public async Task Unsubscribe(string topic)
        {
            if (!mqttClient.IsConnected)
                await Connect();

            await mqttClient.UnsubscribeAsync(topic);
        }

        public async void RecieveMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            string payload = System.Text
                .Encoding
                .UTF8
                .GetString(e.ApplicationMessage.PayloadSegment);

            if (payload.ToLower().StartsWith("rooms"))
                await HandleRoomRequest(payload);
        }

        private async Task HandleRoomRequest(string payload)
        {
            DbContextOptionsBuilder<ApplicationDbContext> options = new();
            options.UseSqlServer(connectionString);
            _context = new ApplicationDbContext(options.Options);

            string RFIDCardId = payload.Split(':')[1];

            if (RFIDCardId == null)
                return;

            var RFIDCard = await _context.RFIDCards
                .Where(rfid => rfid.Id == RFIDCardId)
                .SingleOrDefaultAsync(m => m.Id == RFIDCardId);

            AccessLog accessLog;
            if (RFIDCard == null)
            {
                accessLog = new()
                {
                    FkRFIDCardId = RFIDCardId,
                    Data = $"Card with id {RFIDCardId} was not found in the database",
                };
            }
            else
            {
                var accessesRoomIds = _context.Accesses
                    .Where(a => a.FkRFIDCardId == RFIDCardId)
                    .Select(a => a.FkRoomId)
                    .ToList();

                var result = string.Join(",", accessesRoomIds);

                var cardOwner = _context.CardOwners
                    .Include(c => c.RFIDCard)
                    .SingleAsync(c => c.RFIDCard.Id == RFIDCardId)
                    .Result;

                accessLog = new()
                {
                    FkRFIDCardId = RFIDCardId,
                    Data = $"RFID Card used, result: {result}{(cardOwner != null ? ", card belongs to: " + cardOwner.ToString() : "")}",
                };

                await SendMessage(result);
            }

            _context.Add(accessLog);
            _context.SaveChanges();
        }

        public async Task SendMessage(string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(responseTopic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            if (!mqttClient.IsConnected)
                return;

            try
            {
                await mqttClient.PublishAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured during sending message: {ex.Message}");
            }
        }
    }
}
