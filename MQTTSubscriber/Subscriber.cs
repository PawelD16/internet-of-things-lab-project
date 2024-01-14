using MQTTnet;
using MQTTnet.Client;

var topic = "your/topic";
var port = 1883; // TODO: Change the port, broker adress and topic to valid values
var serverTCP = "broker.hivemq.com";

var mqttFactory = new MqttFactory();
IMqttClient client = mqttFactory.CreateMqttClient();

var options = new MqttClientOptionsBuilder()
    .WithClientId(Guid.NewGuid().ToString())
    .WithTcpServer(serverTCP, port)
    .WithCleanSession()
    .Build();

client.ConnectedAsync += async e =>
{
    Console.WriteLine("### Subscriber connected WITH SERVER ###");
    var topicFilter = new MqttTopicFilterBuilder()
                            .WithTopic(topic)
                            .Build();
    await client.SubscribeAsync(topicFilter);
};

client.DisconnectedAsync += async e =>
{
    Console.WriteLine("### DISCONNECTED FROM SERVER ###");
};

client.ApplicationMessageReceivedAsync += (e) =>
{
    Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
    Console.WriteLine($"+ Payload = {System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
    Console.WriteLine();
    return Task.CompletedTask;
};  

await client.ConnectAsync(options);

Console.WriteLine("Please press a key to disconect the message");

Console.ReadLine();


await client.DisconnectAsync();


Console.WriteLine("Please press a key to exit");

Console.ReadLine();