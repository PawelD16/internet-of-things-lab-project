namespace RemoteLight.Models
{
    public class Broker
    {
        public int BrokerId { get; set; } // Read-only ID
        public string IPAddress { get; set; }

        // Relationships
        public virtual ICollection<Room> Rooms { get; set; }
    }
}
