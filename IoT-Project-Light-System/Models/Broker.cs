namespace IoT_Project_Light_System.Models
{
    public class Broker
    {
        public int BrokerId { get; set; } // Read-only ID
        public string IPAddress { get; set; }

        // Relationships
        public virtual ICollection<Room> Rooms { get; set; }
    }
}
