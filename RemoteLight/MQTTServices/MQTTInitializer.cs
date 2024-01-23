using Microsoft.EntityFrameworkCore;
using RemoteLight.Data;
using RemoteLight.Models;

namespace RemoteLight.MQTTServices
{
    public class MQTTInitializer
    {
        readonly List<MQTTHandler> handlers = new();
        private readonly string _connectionString;
        public MQTTInitializer(string connectionString)
        {
            _connectionString = connectionString;
            DbContextOptionsBuilder<ApplicationDbContext> options = new();
            options.UseSqlServer(connectionString);
            ApplicationDbContext context = new(options.Options);

            foreach (var broker in context.Brokers.ToList())
            {
                handlers.Add(new MQTTHandler(connectionString, broker.IPAddress, broker.Port));
            };
        }

        public void AddBroker(Broker broker)
        {
            handlers.Add(new MQTTHandler(_connectionString, broker.IPAddress, broker.Port));
        }

        public void RemoveBroker(Broker broker)
        {
            handlers.RemoveAll(h => h.ServerIp == broker.IPAddress);
        }
    }
}
