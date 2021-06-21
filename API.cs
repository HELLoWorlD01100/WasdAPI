using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using WasdAPI.Domain;

namespace WasdAPI
{
    public class Api
    {
        public static IEnumerable<UserInfo> SearchUsersByName(string username)
        {
            var uri = $"https://wasd.tv/api/search/channels?limit=15&offset=0&search_phrase={username}";
            var responseContent = SendRequest(uri);
            return JsonParser.SearchUsersByNameParse(responseContent);
        }
        public static string GetIdByName(string username)
        {
            var uri = $"https://wasd.tv/api/v2/broadcasts/public?channel_name={username.ToLower()}";
            var responseContent = SendRequest(uri);
            return JsonParser.GetUserIdFromChannelInfo(responseContent);
        }

        public static bool UserIsOnline(string username)
        {
            var uri = $"https://wasd.tv/api/v2/broadcasts/public?channel_name={username.ToLower()}";
            var responseContent = SendRequest(uri);
            return JsonParser.GetUserIsOnlineFromChannelInfo(responseContent);
        }

        public static bool UserAvailable(string username)
        {
            var uri = $"https://wasd.tv/api/v2/broadcasts/public?channel_name={username.ToLower()}";
            try
            {
                var responseContent = SendRequest(uri);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public static IEnumerable<StreamInfo> GetTopStreams()
        {
            const string uri =
                "https://wasd.tv/api/v2/media-containers?limit=15&offset=0&media_container_status=RUNNING&media_container_type=SINGLE&order_type=VIEWERS&order_direction=DESC";
       
            var responseContent = SendRequest(uri);
       
            return JsonParser.TopStreamsParse(responseContent);
        }

        public static Dictionary<string, string> GetM3U8WithQuality(string userId)
        {
            var m3U8Url = GetM3U8Url(userId);
            using var client = new WebClient();
            var m3U8FileContent = client.DownloadString(m3U8Url);
            return ParseM3U8File(m3U8FileContent);
        }

        private static Dictionary<string, string> ParseM3U8File(string m3U8FileContent)
        {
            var m3U8Regex = new Regex(@"https[\w\W]*?m3u8", RegexOptions.Compiled);
            var resolutionRegex = new Regex(@"RESOLUTION=(\d+x\d+)", RegexOptions.Compiled);
            var m3U8Links = m3U8Regex.Matches(m3U8FileContent).Select(x => x.Value);
            var qualities = resolutionRegex.Matches(m3U8FileContent).Select(x => x.Groups[1].Value);
            return m3U8Links.Zip(qualities, (link, quality) => new {link, quality})
                .ToDictionary(obj => obj.quality, obj => obj.link);
        }

        private static string GetM3U8Url(string userId)
        {
            return $"https://cdn.wasd.tv/live/{userId}/index.m3u8";
        }

        private static string SendRequest(string uri)
        {
            var request = WebRequest.Create(uri);
            request.Method = "GET";
            request.Headers.Add("Referer", "https://wasd.tv/");
            using var response = request.GetResponse();
            using var responseStream = response.GetResponseStream();
        
            if (responseStream is null)
                throw new ArgumentException("Response stream was null.");
        
            using var reader = new StreamReader(responseStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}