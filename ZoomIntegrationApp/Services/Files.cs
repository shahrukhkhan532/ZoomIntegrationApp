namespace ZoomIntegrationApp.Services
{
    public interface IFiles
    {
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
    }
}
