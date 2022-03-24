using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtoNotify
{
    internal class AtoNotifySupervisor
    {
        // Global class variables
        private string ExePath;
        private string ConfigDirPath;
        private string ConfigFilePath;
        private string LogDirPath;
        private string LogFilePath;
        private string SeparationString;

        private string[] ConfigFileContent;
        private string[] ConfigLineContent;
        private int ConfigFileNumberOfLines;
        private bool Verbose = false;

        int i;

        // Constructor
        public AtoNotifySupervisor()
        {
            // Get executable path
            ExePath = AppDomain.CurrentDomain.BaseDirectory;

            // Create configuration & log file names & paths
            if (Environment.OSVersion.ToString().Contains("Windows"))
            {
                ConfigDirPath = ExePath + @"config";
                LogDirPath = ExePath + @"logs";
                ConfigFilePath = ConfigDirPath + @"\AtoNotifyConfig.txt";
                LogFilePath = LogDirPath + @"\AtoNotifyLog.txt";
                SeparationString = " :: ";
            }
            else if (Environment.OSVersion.Platform.ToString().Contains("Linux"))
            {
                ConfigDirPath = ExePath + @"config";
                LogDirPath = ExePath + @"logs";
                ConfigFilePath = ConfigDirPath + @"\AtoNotifyConfig.txt";
                LogFilePath = LogDirPath + @"\AtoNotifyLog.txt";
                SeparationString = " :: ";
            }
            else
            {
                ConfigFilePath = "";
                LogFilePath = "";
                SeparationString = "";
            }
        }

        // Log file functions
        public bool CheckLogFileExistence()
        {
            // Check if config file exists
            return File.Exists(LogFilePath);
        }

        public long ChecklLogFileSize()
        {
            // Check log file size
            long LogFileSize = new FileInfo(LogFilePath).Length;
            return LogFileSize;
        }

        public void CreateLogFile()
        {
            Directory.CreateDirectory(LogDirPath);
            var fileStream = File.Create(LogFilePath);
            fileStream.Close();
            // Zasad ne zapisujemo kada se aplikacija pokrece i zaustavlja
            // ToLogFile("Application has started with new log file");
        }

        public void ArchiveLogFile()
        {
            // Put some info into existing file (the one that will be archived)
            ToLogFile("Log file size is too big. It will be archived and new one will be created");


            // Save file and extend name with current timestamp
            string ArchiveLogFilePath;
            string LogFileTS = DateTime.Now.Year.ToString() + "_" +
                               DateTime.Now.Month.ToString() + "_" +
                               DateTime.Now.Day.ToString() + "_" +
                               DateTime.Now.Hour.ToString() + "_" +
                               DateTime.Now.Minute.ToString() + "_" +
                               DateTime.Now.Second.ToString();

            ArchiveLogFilePath = LogDirPath + @"\AtoNotifyLog_" + LogFileTS + ".txt";

            // Create current log file archive copy
            File.Copy(LogFilePath, ArchiveLogFilePath);
            File.Delete(LogFilePath);
        }

        // Configuration file functions
        public bool CheckConfigFileExistence()
        {
            // Check if config file exists
            return File.Exists(ConfigFilePath);
        }

        public bool ReadConfigFile()
        {
            // This method to read configuration file content
            ConfigFileContent = File.ReadAllLines(ConfigFilePath);

            if (ConfigFileContent.Length > 0)
            {
                // Get number of configuration file lines
                ConfigFileNumberOfLines = File.ReadAllLines(ConfigFilePath).Length;
                // Read verbose mode
                Verbose = GetVerboseMode();
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool GetVerboseMode()
        {
            // This method to get verbose mode from config file content
            try
            {
                i = 0;
                while (i <= ConfigFileNumberOfLines)
                {
                    if (i >= ConfigFileNumberOfLines)
                    {
                        ToLogFile("There is nothing about verbose mode in configuration file. We will assume it's off");
                        return false;
                    }

                    ConfigLineContent = ConfigFileContent[i].Split(SeparationString.ToCharArray());
                    if (ConfigLineContent[0] == "Verbose")
                    {
                        ToConsole("Verbose mode is on. All future messages will be mirrored to console");
                        ToLogFile("Verbose mode is on. All future messages will be mirrored to console");
                        return true;
                    }
                    i++;
                }

                return false;
            }
            catch (Exception ex)
            {
                ToLogFile("Something went wrong with determination of verbose mode. We will assume it's off");
                ToLogFile(ex.Message);
                return false;
            }
        }

        // Logging functions
        public void ToConsole(string message)
        {
            // Output message to console
            string MessageTS = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff");
            Console.WriteLine(MessageTS + " :: " + message);
        }

        public void ToLogFile(string message)
        {
            // Output message to log file
            string MessageTS = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff");
            using (StreamWriter sw = File.AppendText(LogFilePath))
            {
                sw.WriteLine(MessageTS + " :: " + message);
                if (Verbose)
                {
                    Console.WriteLine(MessageTS + " :: " + message);
                }
            }
        }
    }
}
