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
    Console.WriteLine("### PUBLISHER CONNECTED WITH SERVER ###");
    var topicFilter = new MqttTopicFilterBuilder()
                            .WithTopic(topic)
                            .Build();
    await client.SubscribeAsync(topicFilter);
};

client.DisconnectedAsync += e =>
{
    Console.WriteLine("### PUBLISHER DISCONNECTED FROM SERVER ###");
    return Task.CompletedTask;
};

await client.ConnectAsync(options);

Console.WriteLine("Please press a key to publish the message");

Console.ReadLine();

string messagePayload = "Publisher sent you a message!";

var message = new MqttApplicationMessageBuilder()
    .WithTopic(topic)
    .WithPayload(messagePayload)
    .WithRetainFlag()
    .Build();

if (client.IsConnected)
{
    await client.PublishAsync(message);
    Console.WriteLine("Message sent");
}
else
{
    Console.WriteLine("Client not connected");
}

await client.DisconnectAsync();

Console.WriteLine("Please press a key to exit");

Console.ReadLine();