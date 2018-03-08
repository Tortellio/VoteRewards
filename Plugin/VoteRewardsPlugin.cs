using System;
using System.IO;
using System.Net;
using System.Xml;
using Rocket.Core.Plugins;
using Rocket.API.Collections;
using Logger = Rocket.Core.Logging.Logger;

namespace Teyhota.VoteRewards.Plugin
{
    public class VoteRewardsPlugin : RocketPlugin<VoteRewardsConfig>
    {
        public static string PluginName = "VoteRewards";
        public static string PluginVersion = "3.0.0";
        public static string BuildVersion = "12";
        public static string RocketVersion = "4.9.3.0";
        public static string UnturnedVersion = "3.23.5.0";
        public static string ThisDirectory = System.IO.Directory.GetCurrentDirectory() + @"\Plugins\VoteRewards\";

        public static bool CustomKits;
        public static bool Uconomy;
        public static VoteRewardsPlugin Instance;

        public static void Write(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        public static void Write(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        public void CheckForUpdates(string xmlUrl)
        {
            string updateDir = System.IO.Directory.GetCurrentDirectory() + @"\Updates\VoteRewards\";
            string downloadURL = "";
            string newVersion = "";
            string newBuild = "";
            string updateInfo = "";
            XmlTextReader reader = null;

            try
            {
                reader = new XmlTextReader(xmlUrl);
                reader.MoveToContent();
                string elementName = "";

                if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "appinfo"))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            elementName = reader.Name;
                        }
                        else
                        {
                            if ((reader.NodeType == XmlNodeType.Text) && (reader.HasValue))
                            {
                                switch (elementName)
                                {
                                    case "version":
                                        newVersion = reader.Value;
                                        break;
                                    case "build":
                                        newBuild = reader.Value;
                                        break;
                                    case "url":
                                        downloadURL = reader.Value;
                                        break;
                                    case "about":
                                        updateInfo = reader.Value;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                Logger.LogError("Update server down, please try again later\n");
                return;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            if (newVersion == PluginVersion)
            {
                if (newBuild == BuildVersion)
                {
                    return;
                }
            }

            if (!System.IO.Directory.Exists(updateDir))
            {
                System.IO.Directory.CreateDirectory(updateDir);
            }

            if (File.Exists(updateDir + "Update-" + newVersion + ".zip"))
                return;

            try
            {
                new WebClient().DownloadFile(downloadURL, updateDir + "Update-" + newVersion + ".zip");

                Write(string.Format(updateInfo) + "\n", ConsoleColor.Green);
            }
            catch
            {
                Logger.LogError("The update has failed to download\n");
            }
        }
        
        protected override void Load()
        {
            Instance = this;
            
            Write("\n" + PluginName + " " + PluginVersion + " BETA-" + BuildVersion, ConsoleColor.Cyan);
            Write("Made by Teyhota", ConsoleColor.Cyan);
            Write("for Rocket " + RocketVersion + "\n", ConsoleColor.Cyan);

            // update check
            if (Instance.Configuration.Instance.DisableAutoUpdate != "true")
            {
                CheckForUpdates("http://plugins.4unturned.tk/plugins/VoteRewards/update.xml");
            }

            // optional dependencies
            if (File.Exists(System.IO.Directory.GetCurrentDirectory() + @"\Plugins\CustomKits.dll"))
            {
                CustomKits = true;
                Write("Optional dependency CustomKits has been detected", ConsoleColor.Green);
            }
            else
            {
                CustomKits = false;
            }

            if (File.Exists(System.IO.Directory.GetCurrentDirectory() + @"\Plugins\Uconomy.dll"))
            {
                Uconomy = true;
                Write("Optional dependency Uconomy has been detected", ConsoleColor.Green);
            }
            else
            {
                Uconomy = false;
            }
        }

        protected override void Unload()
        {
            Write("Visit Plugins.4Unturned.tk for more!", ConsoleColor.Green);
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"vote_page_msg", "Vote for {0} and receive a reward!"},
                    {"already_voted", "You have already voted in the last 24 hours."},
                    {"not_yet_voted", "You have not voted yet. Type /vote"},
                    {"free_reward", "You gave {0} a free reward!"},
                    {"reward", "You've been rewarded {0}. Thanks for voting!"}
                };
            }
        }
    }
}