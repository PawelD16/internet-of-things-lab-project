using MQTTnet;
using MQTTnet.Client;

var commandTopic = "server/command";
var roomsResultTopic = "server/rooms/results2";
var authResultTopis = "server/auth/results2";
var port = 1883;
var serverTCP = "test.mosquitto.org";

var mqttFactory = new MqttFactory();
IMqttClient client = mqttFactory.CreateMqttClient();

var options = new MqttClientOptionsBuilder()
    .WithClientId(Guid.NewGuid().ToString())
    .WithTcpServer(serverTCP, port)
    .WithCleanSession(false)
    .Build();

client.ConnectedAsync += async e =>
{
    Console.WriteLine("### Subscriber connected WITH SERVER ###");
    var topicFilter = new MqttTopicFilterBuilder()
                            .WithTopic(commandTopic)
                            .Build();
    await client.SubscribeAsync(topicFilter);
};

client.DisconnectedAsync += async e =>
{
    Console.WriteLine("### DISCONNECTED FROM SERVER ###");
};

client.ApplicationMessageReceivedAsync += async (e) =>
{
    var topic = e.ApplicationMessage.Topic;
    if (topic == commandTopic) {
        Console.WriteLine("Received command request.");
        var commandMsg = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        if (commandMsg.Contains(':')) {
            var split = commandMsg.Split(':');
            var command = split[0];
            var argument = split[1];
            switch (command.ToUpper()) {
                case "ROOMS":
                    var uid = argument;
                    var messagePayload = "Rooms answer"; // tutaj zapytanie do bazy o pokoje do ktorych osoba o uid ma dostep
                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic(roomsResultTopic)
                        .WithPayload(messagePayload)
                        .Build();
                    await client.PublishAsync(message);
                    break;
                case "AUTHUSER":
                    var uid2 = argument;
                    var messagePayload2 = "Auth answer"; // 0 or 1 - tutaj zapytanie do bazy czy osoba o danym uid2 ma prawo do zarzadzania
                    var message2 = new MqttApplicationMessageBuilder()
                        .WithTopic(authResultTopis)
                        .WithPayload(messagePayload2)
                        .Build();
                    await client.PublishAsync(message2);
                    break;
            }
        } else {
            Console.WriteLine("invalid command syntax");
        }
    }
    Console.WriteLine();
    // return Task.CompletedTask;
};  

await client.ConnectAsync(options);

Console.WriteLine("Please press a key to disconect the message");

Console.ReadLine();


await client.DisconnectAsync();


Console.WriteLine("Please press a key to exit");

Console.ReadLine();