namespace RemoteLight.MQTTServices;
using global::RemoteLight.Data;
using global::RemoteLight.Models;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class MqttBackgroundService : IHostedService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ConcurrentDictionary<string, IMqttClient> _mqttClients = new();

	private readonly string RECEIVE_TOPIC = "server/commmand";
	private readonly string RESPONSE_TOPIC = "server/result";

	[Obsolete]
	public MqttBackgroundService(IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;
	}	

	[Obsolete]
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await ConnectToBrokersAsync(cancellationToken);
	}

	[Obsolete]
	private async Task ConnectToBrokersAsync(CancellationToken cancellationToken)
	{
		using var scope = _scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var brokers = dbContext.Brokers.ToList();

		var connectionTasks = brokers.Select(broker => ConnectToBrokerAsync(broker, cancellationToken));
		await Task.WhenAll(connectionTasks);

		Console.WriteLine("All broker connection attempts finished.");
	}

	[Obsolete]
	private async Task ConnectToBrokerAsync(Broker broker, CancellationToken cancellationToken)
	{
		try
		{
			var client = new MqttFactory().CreateMqttClient();
			client.ApplicationMessageReceivedAsync += async e =>
			{
				await HandleReceivedApplicationMessageAsync(e, client);
			};

			var options = new MqttClientOptionsBuilder()
				.WithTcpServer(broker.IPAddress, broker.Port)
				.Build();

			await client.ConnectAsync(options, cancellationToken);
			await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(RECEIVE_TOPIC).Build(), cancellationToken: cancellationToken);

			_mqttClients.TryAdd(broker.IPAddress, client);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error connecting to broker {broker.IPAddress}: {ex.Message}");
		}
	}


	public async Task StopAsync(CancellationToken cancellationToken)
	{
		foreach (var client in _mqttClients.Values)
		{
			if (client.IsConnected)
				await client.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken);
		}
	}

	[Obsolete]
	private async Task HandleReceivedApplicationMessageAsync(MqttApplicationMessageReceivedEventArgs e, IMqttClient mqttClient)
	{
		var topic = e.ApplicationMessage.Topic;
		var payload = e.ApplicationMessage.Payload;
		var convertedPayload = Encoding.UTF8.GetString(payload);

		Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
		Console.WriteLine($"+ Topic = {topic}");
		Console.WriteLine($"+ Payload = {payload}");
		Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
		Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");

		using var scope = _scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await ProcessReceivedMessageAsync(dbContext, convertedPayload, mqttClient);
	}

	private async Task ProcessReceivedMessageAsync(ApplicationDbContext dbContext, string message, IMqttClient mqttClient)
	{
		if (message.ToLower().StartsWith("rooms"))
			await HandleRoomAccessRequestMessage(dbContext, message, mqttClient);	
	}

	private async Task HandleRoomAccessRequestMessage(ApplicationDbContext dbContext, string message, IMqttClient mqttClient)
	{
		string RFIDCardId = message.Split(':')[1];
		if (RFIDCardId == null)
			return;

		var RFIDCard = await dbContext.RFIDCards
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
			var accessesRoomIds = dbContext.Accesses.Where(a => a.FkRFIDCardId == RFIDCardId).Select(a => a.FkRoomId).ToList();
			var result = string.Join(",", accessesRoomIds);
			var cardOwner = dbContext.RFIDCards.SingleAsync(c => c.Id == RFIDCardId).Result;

			accessLog = new()
			{
				FkRFIDCardId = RFIDCardId,
				Data = $"RFID Card used, result: {result}{(cardOwner != null ? ", card belongs to: " + cardOwner : "")}",
			};
			await SendMessage(RESPONSE_TOPIC, result, mqttClient);
		}

		dbContext.Add(accessLog);
		await dbContext.SaveChangesAsync();
	}

	public async Task SendMessage(string topic, string payload, IMqttClient mqttClient)
	{
		var message = new MqttApplicationMessageBuilder()
			.WithTopic(topic)
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

