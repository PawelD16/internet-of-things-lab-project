using MQTTnet;
using MQTTnet.Client;
using System.Threading.Channels;

namespace RemoteLight
{
    public class MQTThandler
    {
        private IMqttClient _mqttClient;
        private string _server;
        private int _port;
        

        public MQTThandler(string server="broker.hivemq.com", int port=1883)
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

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

        public async Task Subscribe(string topic="your/topic")
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

        public async Task Unsubscribe(string topic="your/topic")
        {
            if (!_mqttClient.IsConnected)
            {
                await Connect();
            }
            await _mqttClient.UnsubscribeAsync(topic);
        }

        public void RecieveMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
            Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
            Console.WriteLine($"+ Payload = {System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
            Console.WriteLine();
        }
        public async Task SendMessage(string topic = "your/topic", string payload ="message")
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
