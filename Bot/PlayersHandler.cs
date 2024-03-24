using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaPot.Bot.Player;

namespace TeaPot.Bot
{
    internal class PlayersHandler
    {
        private PlayersHandler() { }


        private static Dictionary<ulong, AudioPlayer> players = new Dictionary<ulong, AudioPlayer>();
        private static HashSet<ulong> guildsToWait = new HashSet<ulong>();


        public static bool MakeGuildWait(ulong guildId)
        {
            if (!players.ContainsKey(guildId)) return false;
            guildsToWait.Add(guildId);
            return true;
        }


        public static bool UnmakeGuildWait(ulong guildId)
        {
            if (!guildsToWait.Contains(guildId)) return false;
            guildsToWait.Remove(guildId);
            return true;
        }


        public static void DestroyPlayer(AudioPlayer player)
        {
            players.Remove(player.GuildID);
        }


        public static AudioPlayer? TryGetAudioPlayer(ulong guildId)
        {
            players.TryGetValue(guildId, out var player);
            return player;
        }


        public static string TakeAudioContainer(AudioContainer audioContainer)
        {
            if (guildsToWait.Contains(audioContainer.TargetGuildId)) return "Player stoppign, try later";
            if (players.TryGetValue(audioContainer.TargetGuildId, out var player))
            {
                if (audioContainer.TargetAudioChannelId != player.ChannelID) return "Player is in another channel, can't do that😔";
                player.Add(audioContainer);
                return $"{audioContainer.Title} added to queue, position {player.Length}";
            }
            else
            {
                var newPlayer = new AudioPlayer(audioContainer.TargetGuildId, audioContainer.TargetAudioChannelId);
                newPlayer.Add(audioContainer);
                newPlayer.Play();
                players.Add(audioContainer.TargetGuildId, newPlayer);
                return $"{audioContainer.Title} now playing";
            }
        }

    }
}
