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
        private ApplicationDbContext? _context;

        public string BrokerIP { get; }
        private readonly int brokerPort;
        private readonly string connectionString;
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
            // Rozłączenie z brokerem
            Disconnect().Wait();
        }

        private void InitMqttClientConnection()
        {
            // Podłączenie metody RecieveMessage jako obserwatora wyniku MQTT
            mqttClient.ApplicationMessageReceivedAsync += (e) =>
            {
                RecieveMessage(e);
                return Task.CompletedTask;
            };

            // Połączenie z brokerem i subskrypcja tematu MQTT
            try
            {
                Connect().Wait();
                Subscribe().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to broker {BrokerIP} on port {brokerPort}: {ex.Message}");
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
            // Przyjmowanie ładunku wiadomości MQTT
            string payload = System.Text
                .Encoding
                .UTF8
                .GetString(e.ApplicationMessage.PayloadSegment);

            // Jeżeli komunikat zaczyna się od "rooms",
            // sprawdzenie wiadomości pod kątem obsługi przypadku użycia w programie
            if (payload.ToLower().StartsWith("rooms"))
                await HandleRoomRequest(payload);
        }

        private async Task HandleRoomRequest(string payload)
        {
            // Tworzenie połączenia z bazą danych
            DbContextOptionsBuilder<ApplicationDbContext> options = new();
            options.UseSqlServer(connectionString);
            _context = new ApplicationDbContext(options.Options);

            // Informacja, którą otrzymujemy ma format rooms:<idkarty rfid>, w.p.p. żądanie jest nieprawidłowe i go nie obsługujemy
            string RFIDCardId = payload.Split(':')[1];

            if (RFIDCardId == null)
                return;

            // Znalezienie w bazie danych odpowiedniej karty
            var RFIDCard = await _context.RFIDCards
                .Where(rfid => rfid.Id == RFIDCardId)
                .SingleOrDefaultAsync(m => m.Id == RFIDCardId);

            AccessLog accessLog;
            if (RFIDCard == null)
            {
                // Jeżeli karty o danym numerze nie ma w bazie danych, zapisujemy informacje o próbie weryfikacji i nie odsyłamy odpowiedzi
                accessLog = new()
                {
                    FkRFIDCardId = RFIDCardId,
                    Data = $"Card with id {RFIDCardId} was not found in the database",
                };
            }
            else
            {
                // Jeżeli karta o danym numerze jest w bazie, znajdujemy które pokoje
                // są dla niej dostępne i wysyłamy ich numery, oddzielone przecinkiem
                var accessesRoomIds = _context.Accesses
                    .Where(a => a.FkRFIDCardId == RFIDCardId)
                    .Select(a => a.FkRoomId)
                    .ToList();

                var result = string.Join(",", accessesRoomIds);

                // Znalezienie potencjalnego właściciela karty, aby dodać te informacje do loga
                var cardOwner = _context.CardOwners
                    .Include(c => c.RFIDCard)
                    .SingleAsync(c => c.RFIDCard.Id == RFIDCardId)
                    .Result;

                accessLog = new()
                {
                    FkRFIDCardId = RFIDCardId,
                    Data = $"RFID Card used, result: {result}{(cardOwner != null ? ", card belongs to: " + cardOwner.ToString() : "")}",
                };
                // Wysłanie wiadonmości przez MQTT
                await SendMessage(result);
            }

            // Zapisujemy log w bazie danych
            _context.Add(accessLog);
            _context.SaveChanges();
        }

        public async Task SendMessage(string payload)
        {
            // Wysłanie wiadomości na dany temat, jeżeli jesteśmy połączeni
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
