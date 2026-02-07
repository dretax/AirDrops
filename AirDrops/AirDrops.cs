using System;
using System.Collections.Generic;
using System.IO;
using Fougerite;
using Fougerite.Events;
using Fougerite.Permissions;
using UnityEngine;
using Random = System.Random;

namespace AirDrops
{
    public class AirDrops : Fougerite.Module
    {
        public IniParser Settings;
        public bool TimedAirdrop;
        public int MinPlayers;
        public int AirdropTime;
        public bool Mods;
        public readonly List<string> WLS = new List<string>();
        public int Cooldown;
        public int Chance;
        public bool ModCalltoPos;
        public bool TellDistance;
        
        public const string blue = "[color #0099FF]";
        public const string red = "[color #FF0000]";
        public const string pink = "[color #CC66FF]";
        public const string teal = "[color #00FFFF]";
        public const string green = "[color #009900]";
        public const string purple = "[color #6600CC]";
        public const string white = "[color #FFFFFF]";
        public const string yellow = "[color #FFFF00]";
        
        public readonly Random Randomizer = new Random();

        public readonly Dictionary<string, Vector2> AirDropLocations = new Dictionary<string, Vector2>()
        {
            {"Hacker Valley South", new Vector2(5907,-1848)},
            {"Hacker Mountain South", new Vector2(5268,-1961)},
            {"Hacker Valley Middle", new Vector2(5268,-2700)},
            {"Hacker Mountain North", new Vector2(4529,-2274)},
            {"Hacker Valley North", new Vector2(4416,-2813)},
            {"Wasteland North", new Vector2(3208,-4191)},
            {"Wasteland South", new Vector2(6433,-2374)},
            {"Wasteland East", new Vector2(4942,-2061)},
            {"Wasteland West", new Vector2(3827,-5682)},
            {"Sweden", new Vector2(3677,-4617)},
            {"Everust Mountain", new Vector2(5005,-3226)},
            {"North Everust Mountain", new Vector2(4316,-3439)},
            {"South Everust Mountain", new Vector2(5907,-2700)},
            {"Metal Valley", new Vector2(6825,-3038)},
            {"Metal Mountain", new Vector2(7185,-3339)},
            {"Metal Hill", new Vector2(5055,-5256)},
            {"Resource Mountain", new Vector2(5268,-3665)},
            {"Resource Valley", new Vector2(5531,-3552)},
            {"Resource Hole", new Vector2(6942,-3502)},
            {"Resource Road", new Vector2(6659,-3527)},
            {"Beach", new Vector2(5494,-5770)},
            {"Beach Mountain", new Vector2(5108,-5875)},
            {"Coast Valley", new Vector2(5501,-5286)},
            {"Coast Mountain", new Vector2(5750,-4677)},
            {"Coast Resource", new Vector2(6120,-4930)},
            {"Secret Mountain", new Vector2(6709,-4730)},
            {"Secret Valley", new Vector2(7085,-4617)},
            {"Factory Radtown", new Vector2(6446,-4667)},
            {"Small Radtown", new Vector2(6120,-3452)},
            {"Big Radtown", new Vector2(5218,-4800)},
            {"Hangar", new Vector2(6809,-4304)},
            {"Tanks", new Vector2(6859,-3865)},
            {"Civilian Forest", new Vector2(6659,-4028)},
            {"Civilian Mountain", new Vector2(6346,-4028)},
            {"Civilian Road", new Vector2(6120,-4404)},
            {"Ballzack Mountain", new Vector2(4316,-5682)},
            {"Ballzack Valley", new Vector2(4720,-5660)},
            {"Spain Valley", new Vector2(4742,-5143)},
            {"Portugal Mountain", new Vector2(4203,-4570)},
            {"Portugal", new Vector2(4579,-4637)},
            {"Lone Tree Mountain", new Vector2(4842,-4354)},
            {"Forest", new Vector2(5368,-4434)},
            {"Rad-Town Valley", new Vector2(5907,-3400)},
            {"Next Valley", new Vector2(4955,-3900)},
            {"Silk Valley", new Vector2(5674,-4048)},
            {"French Valley", new Vector2(5995,-3978)},
            {"Ecko Valley", new Vector2(7085,-3815)},
            {"Ecko Mountain", new Vector2(7348,-4100)},
            {"Zombie Hill", new Vector2(6396,-3428)}
        };
        
        public override string Name
        {
            get { return "AirDrops"; }
        }

        public override string Author
        {
            get { return "DreTaX"; }
        }

        public override string Description
        {
            get { return "AirDrops"; }
        }

        public override Version Version
        {
            get { return new Version("1.1"); }
        }

        public override void Initialize()
        {
            ReloadConfig();
            
            if (TimedAirdrop)
            {
                CreateTimer("AirDropTimer", AirdropTime, AirDropCall, true).Start();
            }

            Hooks.OnAirdropCalled += OnAirdropCalled;
            Hooks.OnCommand += OnCommand;
        }

        public override void DeInitialize()
        {
            KillTimers();
            
            Hooks.OnAirdropCalled -= OnAirdropCalled;
            Hooks.OnCommand -= OnCommand;
        }

        private void OnCommand(Fougerite.Player player, string cmd, string[] args)
        {
            if (cmd == "airdrop")
            {
                if (player.Admin || PermissionSystem.GetPermissionSystem().PlayerHasPermission(player, "airdrop.callairdrop") || (player.Moderator && Mods) || WLS.Contains(player.SteamID))
                {
                    if (args.Length == 0)
                    {
                        player.Message("Usage: /airdrop here/random");
                        return;
                    }

                    object ttime = DataStore.GetInstance().Get("AirdropCD", "CD");
                    if (ttime == null)
                    {
                        ttime = 0;
                        DataStore.GetInstance().Add("AirdropCD", "CD", 0);
                    }

                    double time = Convert.ToDouble(ttime);

                    double systick = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
                    double calc = systick - time;

                    if (calc < 0)
                    {
                        time = 0;
                        DataStore.GetInstance().Add("AirdropCD", "CD", 0);
                    }

                    if (calc >= Cooldown || time == 0 || Cooldown == 0)
                    {
                        if (args[0] == "here")
                        {
                            if (player.Admin || PermissionSystem.GetPermissionSystem().PlayerHasPermission(player, "airdrop.callairdrophere") || (player.Moderator && ModCalltoPos) || WLS.Contains(player.SteamID))
                            {
                                World.GetWorld().AirdropAtOriginal(player.X, 700, player.Z);
                                player.Notice("\u2708", "Airdrop has been spawned!", 3);
                                DataStore.GetInstance().Add("AirdropCD", "CD", TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds);
                            }
                        }
                        else if (args[0] == "random")
                        {
                            World.GetWorld().Airdrop();
                            player.Notice("\u2708", "Airdrop has been spawned!", 3);
                            DataStore.GetInstance().Add("AirdropCD", "CD", TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds);
                        }
                    }
                    else
                    {
                        double done = Math.Round(calc);
                        double done2 = Math.Round((double) Cooldown, 2);
                        player.Notice("\u2708", "Cooldown: " + done + "/" + done2 + " seconds.");
                    }
                }
            }
        }

        private void OnAirdropCalled(Vector3 v)
        {
            string pos = CalcPosition(v);
            Server.GetServer().BroadcastFrom("Military", green + "==========================");
            Server.GetServer().BroadcastFrom("Military", green + "Airdrop is headed to: " + teal + pos);
            Server.GetServer().BroadcastFrom("Military", green + "==========================");
            if (TellDistance)
            {
                foreach (var x in Server.GetServer().Players)
                {
                    if (x.IsOnline)
                    {
                        x.MessageFrom("Military", yellow + "Distance from you: " + Vector3.Distance(x.Location, v));
                    }
                }
            }
        }

        private string CalcPosition(Vector3 v)
        {
            float closest = float.MaxValue;
            Vector2 pos = new Vector2(v.x, v.z);
            string name = "Unknown";
            foreach (var x in AirDropLocations)
            {
                float cdist = Vector2.Distance(x.Value, pos);
                if (cdist < closest)
                {
                    closest = cdist;
                    name = x.Key;
                }
            }

            return name;
        }

        private void AirDropCall(TimedEvent te)
        {
            if (Server.GetServer().Players.Count >= MinPlayers)
            {
                int random = Randomizer.Next(1, 101);
                if (random <= Chance || Chance == 0)
                {
                    World.GetWorld().Airdrop();
                }
                else
                {
                    Server.GetServer().BroadcastFrom("Military", red + "We failed to drop the Airdrop at a location!");
                }
            }
            else
            {
                Server.GetServer().BroadcastFrom("Military", red + "HQ needs atleast " + white + MinPlayers + red
                                                             + " soldiers on the ground!");
                Server.GetServer().BroadcastFrom("Military",
                    red + "We will check back in after " + white + (AirdropTime / 60000)
                    + red + " minutes!");
            }
        }

        private void ReloadConfig()
        {
            try
            {
                if (!File.Exists(Path.Combine(ModuleFolder, "Settings.ini")))
                {
                    File.Create(Path.Combine(ModuleFolder, "Settings.ini")).Dispose();
                    Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                    Settings.AddSetting("Settings", "TimedAirdrop", "True");
                    Settings.AddSetting("Settings", "AirdropTime", "1800000");
                    Settings.AddSetting("Settings", "MinPlayers", "20");
                    Settings.AddSetting("Settings", "Mods", "20");
                    Settings.AddSetting("Settings", "WLS", "SteamIDHere,SteamID2Here,SteamID3Here");
                    Settings.AddSetting("Settings", "Cooldown", "1800000");
                    Settings.AddSetting("Settings", "Chance", "47");
                    Settings.AddSetting("Settings", "ModCalltoPos", "False");
                    Settings.AddSetting("Settings", "TellDistance", "True");
                    Settings.Save();
                }
                else
                {
                    Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                }

                TimedAirdrop = Settings.GetBoolSetting("Settings", "TimedAirdrop");
                ModCalltoPos = Settings.GetBoolSetting("Settings", "ModCalltoPos");
                TellDistance = Settings.GetBoolSetting("Settings", "TellDistance");
                Mods = Settings.GetBoolSetting("Settings", "Mods");
                AirdropTime = int.Parse(Settings.GetSetting("Settings", "AirdropTime"));
                MinPlayers = int.Parse(Settings.GetSetting("Settings", "MinPlayers"));
                Cooldown = int.Parse(Settings.GetSetting("Settings", "Cooldown"));
                Chance = int.Parse(Settings.GetSetting("Settings", "Chance"));

                Cooldown = Cooldown / 60000 * 60; // Do not edit this line.

                string data = Settings.GetSetting("Settings", "WLS");
                WLS.Clear();
                foreach (var x in data.Split(','))
                {
                    WLS.Add(x);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("[AirDrops] Error while reading the config: " + ex);
            }
        }
    }
}