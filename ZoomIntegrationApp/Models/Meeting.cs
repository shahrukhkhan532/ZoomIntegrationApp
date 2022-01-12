namespace ZoomIntegrationApp.Models
{
    public class Meeting
    {
        public string Topic { get; set; }
        public string Agenda { get; set; }
        public DateTime Date { get; set; }
        public int Time { get; set; }
        public int Duration { get; set; }
    }
}
