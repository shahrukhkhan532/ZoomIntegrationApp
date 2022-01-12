namespace ZoomIntegrationApp.Models
{
    public class Root
    {
        public int page_size { get; set; }
        public int total_records { get; set; }
        public string next_page_token { get; set; }
        public List<MeetingResponce> meetings { get; set; }
    }
}
