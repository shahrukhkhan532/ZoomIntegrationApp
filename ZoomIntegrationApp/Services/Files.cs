namespace ZoomIntegrationApp.Services
{
    public interface IFiles
    {
        void AllMeeting(string result);
        void DeleteMeeting(string result);
        void MeetingDetails(string result);
        void WriteUserDetails(string result);
    }
    public class Files : IFiles
    {
        private readonly IWebHostEnvironment _environment;

        public Files(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public void WriteUserDetails(string result)
        {
            string directoryPath = _environment.WebRootPath + "\\sitemaps\\";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string filepath = directoryPath + "\\usersDetails.json";
            File.WriteAllText(filepath, result);
        }
        public void MeetingDetails(string result)
        {
            string directoryPath = _environment.WebRootPath + "\\sitemaps\\";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string filepath = directoryPath + "\\Meeting.json";
            File.WriteAllText(filepath, result);
        }
        public void AllMeeting(string result)
        {
            string directoryPath = _environment.WebRootPath + "\\sitemaps\\";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string filepath = directoryPath + "\\AllMeeting.json";
            File.WriteAllText(filepath, result);
        }
        public void DeleteMeeting(string result)
        {
            string directoryPath = _environment.WebRootPath + "\\sitemaps\\";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string filepath = directoryPath + "\\Delete.json";
            File.WriteAllText(filepath, result);
        }
    }
}
