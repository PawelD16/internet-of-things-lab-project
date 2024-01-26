using Microsoft.EntityFrameworkCore;
using RemoteLight.Data;
using RemoteLight.Models;

namespace RemoteLight.MQTTServices
{
    public class MQTTInitializer
    {
        private readonly List<MQTTHandler> handlers = new();

        // temat, na którym serwer nasłuchuje zapytań
        private readonly string RECEIVE_TOPIC = "server/command";

        // temat, na którym serwera odpowiada
        private readonly string RESPONSE_TOPIC = "server/result";

        public MQTTInitializer(string connectionString)
        {
            DbContextOptionsBuilder<ApplicationDbContext> options = new();
            options.UseSqlServer(connectionString);
            ApplicationDbContext context = new(options.Options);

            // Dla każdego skonfigurowanego brokera tworzona instancja klienta MQTT 
            foreach (var broker in context.Brokers.ToList())
            {
                handlers.Add(new MQTTHandler(connectionString, broker.IPAddress, broker.Port, RECEIVE_TOPIC, RESPONSE_TOPIC));
            };
        }
    }
}
