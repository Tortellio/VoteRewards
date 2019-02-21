using System.Collections.Generic;
using System.Xml.Serialization;
using Rocket.API;

namespace Teyhota.VoteRewards.Plugin
{
    public class VoteRewardsConfig : IRocketPluginConfiguration
    {
        public static VoteRewardsConfig Instance;

        public string DisableAutoUpdate;
        public string VotePageURL;
        public bool AlertOnJoin;
        public bool GlobalAnnouncement;
        public List<Reward> Rewards;
        public List<Service> Services;

        public class Reward
        {
            public Reward() { }

            internal Reward(string type, string value, short chance)
            {
                Type = type;
                Value = value;
                Chance = chance;
            }

            [XmlAttribute]
            public string Type;
            [XmlAttribute]
            public string Value;
            [XmlAttribute]
            public short Chance;
        }
        public class Service
        {
            public Service() { }

            internal Service(string name, string apiKey)
            {
                Name = name;
                APIKey = apiKey;
            }

            [XmlAttribute]
            public string Name;
            [XmlAttribute]
            public string APIKey;
        }

        public void LoadDefaults()
        {
            Instance = this;
            DisableAutoUpdate = "true";
            VotePageURL = "https://unturned-servers.net/my_server_vote_page";
            AlertOnJoin = true;
            GlobalAnnouncement = true;
            Rewards = new List<Reward>()
            {
                new Reward("item", "235,236,237,238,253,1369,1371,1371,297,298,298,298,15,15,15,15,15", 40),
                new Reward("xp", "1400", 50),
                new Reward("group", "VIP", 10)
            };
            Services = new List<Service>()
            {
                new Service("unturned-servers", ""),
                new Service("unturnedsl", ""),
                new Service("obs.erve.me", "")
            };
        }
    }
}