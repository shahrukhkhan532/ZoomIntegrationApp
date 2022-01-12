namespace ZoomIntegrationApp.Models;

public class ZoomUser
{
    public string id { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string email { get; set; }
    public DateTime created_at { get; set; }
    public string pic_url { get; set; }
    public string account_id { get; set; }
    public string phone_number { get; set; }
    public string status { get; set; }
}
