using System.Collections.Generic;
using System.Diagnostics;
using Rocket.Unturned.Player;
using Rocket.Unturned.Chat;
using Rocket.API;
using SDG.Unturned;

namespace Teyhota.VoteRewards.Commands
{
    public class Command_Vote : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "vote";

        public string Help => "Vote for the server to receive a reward";

        public string Syntax => "";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "voterewards.vote" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            
            if (command.Length == 0)
            {
                player.Player.askBrowserRequest(player.CSteamID, Plugin.VoteRewardsPlugin.Instance.Translate("vote_page_msg", Provider.serverName), Plugin.VoteRewardsPlugin.Instance.Configuration.Instance.VotePageURL);
            }
            else
            {
                if (command[0] == "force")
                {
                    UnturnedChat.Say(caller, Plugin.VoteRewardsPlugin.Instance.Translate("vote_page_msg", Provider.serverName));
                    Process.Start(Plugin.VoteRewardsPlugin.Instance.Configuration.Instance.VotePageURL);
                }
            }
        }
    }
}
