using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using RemoteLight.Data;
using System.Threading.Channels;

namespace RemoteLight
{
    public class MQTThandler
    {
        private IMqttClient _mqttClient;
        private string _server;
        private int _port;
        private string _conectionString;
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

            DbContextOptionsBuilder<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>();
            options.UseSqlServer(_conectionString);
            _context = new ApplicationDbContext(options.Options);

            if (payload.StartsWith("rooms"))
            {
                String RFIDCardId = payload.Split(':')[1];
                var RFIDCard = await _context.RFIDCards
                .Where(rfid => rfid.Id == RFIDCardId)
                .SingleOrDefaultAsync(m => m.Id == RFIDCardId);

                if (RFIDCard == null)
                {
                    Console.WriteLine("Card has not been found.");
                }
                else
                {
                    var accessesRoomIds = _context.Accesses.Where(a => a.RFIDId == RFIDCardId).Select(a => a.RoomId).ToList();
                    var result = string.Join(",", accessesRoomIds);
                    Console.WriteLine(result);
                    await SendMessage("server/result", result);
                }
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
