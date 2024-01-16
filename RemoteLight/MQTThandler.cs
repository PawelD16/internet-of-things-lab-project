using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using RemoteLight.Data;
using RemoteLight.Models;
using System.Threading.Channels;

namespace RemoteLight
{
    public class MQTThandler
    {
        private readonly IMqttClient _mqttClient;
        private readonly string _server;
        private readonly int _port;
        private readonly string _conectionString;
        private ApplicationDbContext _context;

        public MQTThandler(string conectionString, string server="broker.hivemq.com", int port=1883)
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
            _conectionString = conectionString;

            _mqttClient.ApplicationMessageReceivedAsync += (e) =>
            {
                RecieveMessage(e);
                return Task.CompletedTask;
            };

            _mqttClient.DisconnectedAsync += e =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                return Task.CompletedTask;
            };

            _mqttClient.ConnectedAsync += e =>
            {
                Console.WriteLine("### CONNECTED TO SERVER ###");
                return Task.CompletedTask;
            };

            _server = server;
            _port = port;
            Connect().Wait();
            Subscribe().Wait();
        }

        ~MQTThandler()
        {
             Disconnect().Wait();
        }

        public async Task Connect()
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(_server, _port)
                .WithCleanSession()
                .Build();

            await _mqttClient.ConnectAsync(options, CancellationToken.None);
        }

        public async Task Disconnect()
        {
            await _mqttClient.DisconnectAsync();
        }

        public async Task Subscribe(string topic="server/command")
        {
            if (!_mqttClient.IsConnected)
            {
                await Connect();
            }
            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build();
            await _mqttClient.SubscribeAsync(topicFilter);
        }

        public async Task Unsubscribe(string topic="server/command")
        {
            if (!_mqttClient.IsConnected)
            {
                await Connect();
            }
            await _mqttClient.UnsubscribeAsync(topic);
        }

        public async void RecieveMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
            Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
            String payload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            Console.WriteLine($"+ Payload = {payload}");
            Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");

            DbContextOptionsBuilder<ApplicationDbContext> options = new();
            options.UseSqlServer(_conectionString);
            _context = new ApplicationDbContext(options.Options);

            if (payload.StartsWith("rooms"))
            {
                String RFIDCardId = payload.Split(':')[1];
                var RFIDCard = await _context.RFIDCards
                .Where(rfid => rfid.Id == RFIDCardId)
                .SingleOrDefaultAsync(m => m.Id == RFIDCardId);

                AccessLog accessLog;
                if (RFIDCard == null)
                {
                    Console.WriteLine("Card has not been found.");
                    accessLog = new()
                    {

                        FkRFIDCardId = RFIDCardId,
                        Data = $"Card with id {RFIDCardId} was not found in the database",
                    };
                }
                else
                {
                    var accessesRoomIds = _context.Accesses.Where(a => a.FkRFIDId == RFIDCardId).Select(a => a.FkRoomId).ToList();
                    var result = string.Join(",", accessesRoomIds);
                    var cardOwner = _context.RFIDCards.SingleAsync(c => c.Id == RFIDCardId).Result;

                    accessLog = new()
                    {

                        FkRFIDCardId = RFIDCardId,
                        Data = $"RFID Card used, result: {result}{(cardOwner != null ? ", card belongs to: " + cardOwner : "")}",
                    };
                    _context.Add(accessLog);
                    Console.WriteLine(result);
                    await SendMessage("server/result", result);
                }

                _context.Add(accessLog);
                _context.SaveChanges();

            }
        }
        public async Task SendMessage(string topic ="server/result", string payload ="message")
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            if(_mqttClient.IsConnected)
            {
                try
                {
                    await _mqttClient.PublishAsync(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occured during sending message: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Client is not connected");
            }

        }
    }
}
