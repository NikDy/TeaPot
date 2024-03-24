using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TeaPot.Bot
{
    internal class BotToolign
    {
        public static bool IsHttpOrHttpsValid(string url)
        {
            Uri uriResult;
            if (Uri.TryCreate(url, UriKind.Absolute, out uriResult))
            {
                return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
            }
            return false;
        }


        public static bool IsYoutubeLink(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.youtu.be/Ddn4MGaS3N4");
            request.Method = "HEAD";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Console.WriteLine("Does this resolve to youtube?: {0}", response.ResponseUri.ToString().Contains("youtube.com") ? "Yes" : "No");
            }
            return true;
        }

    }
}
