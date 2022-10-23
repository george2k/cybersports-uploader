using System.IO;
using CommandLine;

namespace Basketball
{
    public class Options
    {
        [Option("WatchFolder", Required = true, HelpText = "Folder to monitor for changes")]
        public string WatchFolder { get; set; }

        [Option("OutputFolder", Required = true, HelpText = "Folder where to store files to upload")]
        public string OutputFolder { get; set; }
        
        public string LiveHtmlPath => Path.Combine(OutputFolder, "live.html");
        public string BoxHtmlPath => Path.Combine(OutputFolder, "box.html");

        [Option("LeftImageUrl", Required = false, HelpText = "Url for the image to show on the left hand side")]
        public string LeftImagePath { get; set; }
        [Option("RightImagePath", Required = false, HelpText = "Url for the image to show on the right hand side")]
        public string RightImagePath { get; set; }
        [Option("CenterHtml", Required = false, HelpText = "Html for the center")]
        public string CenterHtml { get; set; }

        [Option("FTP.Host", Required = true, HelpText = "FTP Server to upload the web cast")]
        public string FtpHost { get; set; }
        [Option("FTP.Folder", Required = false, HelpText = "Path on the FTP server where the files will be uploaded")]
        public string FtpFolder { get; set; }
        [Option("FTP.LiveFileName", Required = false, HelpText = "The file name to upload the live stats as", Default = "live.html")]
        public string FtpLiveFile { get; set; }
        [Option("FTP.BoxFileName", Required = false, HelpText = "The file name to upload the box score as", Default = "box.html")]
        public string FtpBoxFile { get; set; }
        [Option("FTP.Username", Required = true, HelpText = "The username to user to connect to the FTP server")] 
        public string FtpUsername { get; set; }
        [Option("FTP.Password", Required = true, HelpText = "The password to connect to the FTP server")] 
        public string FtpPassword { get; set; }

        [Option("AnalyticId", Required = false, HelpText = "Google Analytic Id to use for the web cast, if not provided it will not use analytics")]
        public string AnalyticId { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
    }
}
