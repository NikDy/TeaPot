using Discord.Audio.Streams;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaPot.Bot.Player
{
    internal class FFMPEGConverter
    {

        public static async Task<Stream> Convert(Stream audioStream)
        {
            MemoryStream stream = new MemoryStream();
            using (var ffmpeg = CreateProcess())
            {
                Task copyTask = audioStream.CopyToAsync(ffmpeg.StandardInput.BaseStream).ContinueWith(x => ffmpeg.StandardInput.Close());
                Task readTask = ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream);
                await Task.WhenAll(readTask, copyTask);
            }
            stream.Position = 0;
            return stream;
        }


        private static Process? CreateProcess()
        {
            if (Environment.OSVersion.Platform is PlatformID.Win32NT or PlatformID.Unix)
            {
                return Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                });
            }           
            return null;
        }

    }
}
