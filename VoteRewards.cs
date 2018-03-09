using System.Net;
using System.Linq;
using System.Collections.Generic;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.Core.Plugins;
using Rocket.Core;
using Rocket.API;
using SDG.Unturned;
using UnityEngine;
using fr34kyn01535.Uconomy;
using Teyhota.CustomKits;
using Logger = Rocket.Core.Logging.Logger;

namespace Teyhota.VoteRewards
{
    public class VoteRewards
    {
        public class Result
        {
            public string service;
            public string result;

            public Result(string service, string result)
            {
                this.service = service;
                this.result = result;
            }
        }

        public static Result GetVote(UnturnedPlayer player)
        {
            WebClient wc = new WebClient();
            string result = null;

            foreach (var service in Plugin.VoteRewardsPlugin.Instance.Configuration.Instance.Services)
            {
                if (service.APIKey.Length == 0)
                {
                    Logger.LogError("You need to setup API key(s) in your Config file.");
                    break;
                }

                string url = "";

                if (service.Name.Contains("unturned-servers"))
                {
                    url = "https://unturned-servers.net/api/?object=votes&element=claim&key={0}&steamid={1}";
                }
                else if (service.Name.Contains("unturnedsl"))
                {
                    url = "http://unturnedsl.com/api/dedicated/{0}/{1}";
                }
                else if (service.Name.Contains("obs.erve.me") || service.Name.Contains("observatory"))
                {
                    url = "http://api.observatory.rocketmod.net/?server={0}&steamid={1}";
                }

                try
                {
                    result = wc.DownloadString(string.Format(url, service.APIKey, player.CSteamID.m_SteamID));

                    if (result.Length > 1 || result.Length < 1)
                    {
                        Logger.LogError("an error has occurred, please report it!");
                        break;
                    }

                    return new Result(service.Name, result);
                }
                catch (WebException)
                {
                    Logger.LogError("Could not connect to API");
                    break;
                }
            }

            return null;
        }

        public static void SetVote(UnturnedPlayer player)
        {
            WebClient wc = new WebClient();
            string result = null;

            foreach (var service in Plugin.VoteRewardsPlugin.Instance.Configuration.Instance.Services)
            {
                string url = "";

                if (service.Name.Contains("unturned-servers"))
                {
                    url = "https://unturned-servers.net/api/?action=post&object=votes&element=claim&key={0}&steamid={1}";
                }
                else if (service.Name.Contains("unturnedsl"))
                {
                    url = "http://unturnedsl.com/api/dedicated/post/{0}/{1}";
                }
                else if (service.Name.Contains("obs.erve.me") || service.Name.Contains("observatory"))
                {
                    url = "http://api.observatory.rocketmod.net/?server={0}&steamid={1}&claim";
                }

                try
                {
                    result = wc.DownloadString(string.Format(url, service.APIKey, player.CSteamID.m_SteamID));

                    if (result.Length > 1 || result.Length < 1)
                    {
                        Logger.LogError("an error has occurred, please report it!");
                        break;
                    }
                }
                catch (WebException)
                {
                    Logger.LogError("Could not connect to API");
                    break;
                }
            }

            switch (result)
            {
                case "0": // Not claimed
                    Logger.LogError("an error has occurred, please report it!");
                    break;
                case "1": // Claimed
                    break;
            }
        }

        public static void HandleVote(UnturnedPlayer player)
        {
            Result voteResult = GetVote(player);

            switch (voteResult.result)
            {
                case "0": // Hasn't voted
                    UnturnedChat.Say(player, Plugin.VoteRewardsPlugin.Instance.Translate("not_yet_voted", voteResult.service), Color.red);
                    break;
                case "1": // Has voted
                    GiveReward(player, voteResult.service);
                    SetVote(player);
                    break;
                case "2": // Has voted & claimed
                    UnturnedChat.Say(player, Plugin.VoteRewardsPlugin.Instance.Translate("already_voted", voteResult.service), Color.red);
                    break;
            }
        }

        public static void GiveItem(UnturnedPlayer player, Item item)
        {
            player.Inventory.tryAddItem(item, true, true);
        }

        public static void GiveReward(UnturnedPlayer player, string serviceName = null)
        {
            int sum = Plugin.VoteRewardsPlugin.Instance.Configuration.Instance.Rewards.Sum(p => p.Chance);
            string selectedElement = null;
            string value = null;

            System.Random r = new System.Random();

            int i = 0, diceRoll = r.Next(0, sum);

            foreach (var reward in Plugin.VoteRewardsPlugin.Instance.Configuration.Instance.Rewards)
            {
                if (diceRoll > i && diceRoll <= i + reward.Chance)
                {
                    selectedElement = reward.Type;
                    value = reward.Value;
                    break;
                }
                i = i + reward.Chance;
            }

            if (selectedElement == null || value == null)
            {
                Logger.LogError("an error has occurred, please report it!");
                return;
            }

            switch (selectedElement)
            {
                case "item":
                    {
                        List<string> items = value.Split(',').ToList();
                        foreach (string item in items)
                        {
                            ushort itemID = ushort.Parse(item);
                            GiveItem(player, new Item(itemID, true));
                        }
                        UnturnedChat.Say(player, Plugin.VoteRewardsPlugin.Instance.Translate("reward", "some items"));
                    }
                    break;
                case "xp":
                    {
                        player.Experience += uint.Parse(value);
                        UnturnedChat.Say(player, Plugin.VoteRewardsPlugin.Instance.Translate("reward", value + " xp"));
                    }
                    break;
                case "group":
                    {
                        R.Permissions.AddPlayerToGroup(value, player);
                        R.Permissions.Reload();

                        UnturnedChat.Say(player, Plugin.VoteRewardsPlugin.Instance.Translate("reward", value + " Permission Group"));
                    }
                    break;
                case "uconomy":
                    {
                        if (!Plugin.VoteRewardsPlugin.Uconomy)
                        {
                            Logger.LogError("you must install Uconomy first!");
                            return;
                        }

                        RocketPlugin.ExecuteDependencyCode("Uconomy", (IRocketPlugin plugin) =>
                        {
                            Uconomy.Instance.Database.IncreaseBalance(player.CSteamID.ToString(), decimal.Parse(value));

                            UnturnedChat.Say(player, Plugin.VoteRewardsPlugin.Instance.Translate("reward", value + " Uconomy " + Uconomy.Instance.Configuration.Instance.MoneyName + "s"));
                        });
                    }
                    break;
                case "slot":
                    {
                        if (!Plugin.VoteRewardsPlugin.CustomKits)
                        {
                            Logger.LogError("you must install CustomKits first!");
                            return;
                        }

                        RocketPlugin.ExecuteDependencyCode("CustomKits", (IRocketPlugin plugin) =>
                        {
                            SlotManager.AddSlot(player, 1, int.Parse(value));

                            UnturnedChat.Say(player, Plugin.VoteRewardsPlugin.Instance.Translate("reward", "a CustomKits slot with item limit of " + value ));
                        });
                    }
                    break;
            }

            if (Plugin.VoteRewardsPlugin.Instance.Configuration.Instance.GlobalAnnouncement)
            {
                UnturnedChat.Say(Plugin.VoteRewardsPlugin.Instance.Translate("broadcast_reward", player.CharacterName, serviceName));
            }
        }
    }
}
