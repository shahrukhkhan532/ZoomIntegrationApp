namespace ZoomIntegrationApp.Models;

public class Zoom
{
    public string ClientID { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUrl { get; set; }
    public string AuthorizationUrl { get; set; }
    public string AccessTokenUrl { get; set; }
}
