using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using CommandLine;

namespace Basketball
{
    class Program
    {
        private static readonly object LiveLock = new object();
        private static readonly object BoxLock = new object();

        static void Main(string[] args)
        {
            Version version = Assembly.GetEntryAssembly()?.GetName().Version;

            if (version != null)
            {
                Console.Write($"Version: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}");
            }

            var parserResult = Parser.Default.ParseArguments<Options>(args);
            if (parserResult.Tag == ParserResultType.Parsed)
            {
                Run(parserResult.Value);
            }
            else
            {
                DisplayGui();
            }
        }

        private static void DisplayGui()
        {
            var mainForm = new MainForm();
            mainForm.ShowDialog();
        }

        static void HandleParseError(IEnumerable<Error> errors)
        {
            Console.Error.WriteLine("One or more errors found while parsing command lines:");
            foreach (Error error in errors)
            {
                Console.Error.WriteLine($" - {error}");
            }
        }

        static void Run(Options options)
        {
            if (!Directory.Exists(options.OutputFolder))
            {
                Directory.CreateDirectory(options.OutputFolder);
            }

            FileSystemWatcher watcher = new FileSystemWatcher();

            watcher.Path = options.WatchFolder;
            watcher.EnableRaisingEvents = true;

            watcher.Created += (sender, eventArgs) => UpdateGames(eventArgs, options);
            //watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);

            watcher.Changed += (sender, eventArgs) => UpdateGames(eventArgs, options);
            //watcher.Renamed += new RenamedEventHandler(watcher_Renamed);
            if (options.Verbose)
            {
                Console.WriteLine("FileSystemWatcher ready and listening to changes in:" + watcher.Path);
            }

            new System.Threading.AutoResetEvent(false).WaitOne();
        }

        static void UpdateGames(FileSystemEventArgs e, Options options)
        {
            if (e.Name.EndsWith("asp", StringComparison.OrdinalIgnoreCase))
            {
                lock (LiveLock)
                {
                    try
                    {
                        File.Copy(e.FullPath, options.LiveHtmlPath, true);
                        if (options.Verbose)
                        {
                            Console.WriteLine($"{DateTime.Now:T} - Moved live");
                        }

                        string[] originalLines = File.ReadAllLines(options.LiveHtmlPath);
                        if (originalLines.Length < 200)
                        {
                            return;
                        }
                        string[] newLines = new string[originalLines.Length];
                        Array.Copy(originalLines, 12, newLines, 0, originalLines.Length - 12);
                        newLines[5] = newLines[5].Replace(".asp", ".css");
                        if (!string.IsNullOrEmpty(options.AnalyticId))
                        {
                            newLines[5] += $"{Environment.NewLine}<script async src=\"https://www.googletagmanager.com/gtag/js?id={options.AnalyticId}\"></script><script>window.dataLayer = window.dataLayer || [];function gtag(){{dataLayer.push(arguments);}}gtag('js', new Date());gtag('config', '{options.AnalyticId}');</script>";
                        }
                        newLines[14] = $"{options.LeftImagePath}</td>";
                        newLines[16] = options.CenterHtml;
                        newLines[18] = $"{options.RightImagePath}</td>";
                        newLines[74] = $"<!-- {newLines[74]} -->";
                        newLines[75] = $"<!-- {newLines[75]} -->";
                        newLines[76] = $"<!-- {newLines[76]} -->";
                        newLines[83] = $"<!-- {newLines[83]} -->";
                        newLines[84] = $"<!-- {newLines[84]} -->";
                        newLines[85] = $"<!-- {newLines[85]} -->";
                        for (int i = newLines.Length - 42; i <= newLines.Length - 42 + 26; i++)
                        {
                            newLines[i] = $"<!-- {newLines[i]} -->";
                        }
                        newLines[114] = $"<a CLASS=\"StatCentral\" href=\"{options.FtpBoxFile}\">Box Score</a> </a>";
                        File.WriteAllLines(options.LiveHtmlPath, newLines);
                        if (options.Verbose)
                        {
                            Console.WriteLine($"{DateTime.Now:T} - Edited live");
                        }

                        using (WebClient liveClient = new WebClient())
                        {
                            liveClient.Credentials = new NetworkCredential(options.FtpUsername, options.FtpPassword);
                            liveClient.UploadFile($"ftp://{options.FtpHost}/{options.FtpFolder}/{options.FtpLiveFile}", WebRequestMethods.Ftp.UploadFile, options.LiveHtmlPath);
                            Console.WriteLine($"{DateTime.Now:T} - Uploaded live");
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }

            if (e.Name.EndsWith("bx.html", StringComparison.OrdinalIgnoreCase))
            {
                lock (BoxLock)
                {
                    try
                    {
                        File.Copy(e.FullPath, options.BoxHtmlPath, true);
                        if (options.Verbose)
                        {
                            Console.WriteLine($"{DateTime.Now:T} - Moved box");
                        }
                        using (WebClient liveClient = new WebClient())
                        {
                            liveClient.Credentials = new NetworkCredential(options.FtpUsername, options.FtpPassword);
                            string ftpPath = $"ftp://{options.FtpHost}/";
                            if (!string.IsNullOrEmpty(options.FtpFolder))
                            {
                                ftpPath += $"{options.FtpFolder}/";
                            }
                            ftpPath += options.FtpBoxFile;
                            liveClient.UploadFile($"ftp://{options.FtpHost}/{options.FtpFolder}/{options.FtpBoxFile}", WebRequestMethods.Ftp.UploadFile, options.BoxHtmlPath);
                            Console.WriteLine($"{DateTime.Now:T} - Uploaded box");
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }

        }
    }
}
