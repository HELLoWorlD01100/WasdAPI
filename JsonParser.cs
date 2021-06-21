﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using WasdAPI.Domain;

namespace WasdAPI
{
    public class JsonParser
    {
        public static IEnumerable<UserInfo> SearchUsersByNameParse(string responseContent)
        {
            var jObj = JObject.Parse(responseContent);

            if (!jObj.ContainsKey("result") || !(jObj["result"] is JObject result))
                throw new ArgumentException($"Wrong response. {jObj}");
            
            if (!result.TryGetValue("count", out var countToken))
                throw new ArgumentException($"Wrong response. {result}");
            
            if (!int.TryParse(countToken.ToString(), out var count))
                throw new ArgumentException($"Wrong response. {countToken}");
            
            // if (count == 0)
            //     return Enumerable.Empty<UserInfo>();
            
            if (!result.ContainsKey("rows") || !(result["rows"] is JArray rows))
                throw new ArgumentException($"Wrong response. {result}");
            
            foreach(var rowToken in rows)
            {
                if (!(rowToken is JObject row))
                    throw new ArgumentException($"Wrong response. {result}");
                
                yield return GetUserInfo(row);
            }
        }

        private static UserInfo GetUserInfo(JObject user)
        {
            if (!user.TryGetValue("user_id", out var id))
                throw new ArgumentException($"Wrong response. {user}");
            
            if (!user.TryGetValue("channel_name", out var name))
                throw new ArgumentException($"Wrong response. {user}");
            
            if (!user.TryGetValue("followers_count", out var followersCountToken))
                throw new ArgumentException($"Wrong response. {user}");
            
            if (!int.TryParse(followersCountToken.ToString(), out var followersCount))
                throw new ArgumentException($"Wrong response. {user}");
            
            if (!user.TryGetValue("channel_is_live", out var isLiveToken))
                throw new ArgumentException($"Wrong response. {user}");
            
            if (!bool.TryParse(isLiveToken.ToString(), out var isLive))
                throw new ArgumentException($"Wrong response. {user}");
            
            if (!user.ContainsKey("channel_image") || !(user["channel_image"] is JObject channelImages))
                throw new ArgumentException($"Wrong response. {user}");
            //channel_description
            
            if (!user.TryGetValue("channel_description", out var description))
                throw new ArgumentException($"Wrong response. {user}");
            
            if (!channelImages.TryGetValue("medium", out var profileImageUrl))
                throw new ArgumentException($"Wrong response. {channelImages}");

            return new UserInfo(name.ToString(), followersCount, profileImageUrl.ToString(), description.ToString(),
                followersCount, isLive);
        }
        
        public static bool GetUserIsOnlineFromChannelInfo(string responseContent)
        {
            var jObj = JObject.Parse(responseContent);

            if (!jObj.ContainsKey("result") || !(jObj["result"] is JObject result))
                throw new ArgumentException($"Wrong response. {jObj}");
            
            if (!result.ContainsKey("channel") || !(result["channel"] is JObject channel))
                throw new ArgumentException($"Wrong response. {result}");
            
            if (!channel.TryGetValue("channel_is_live", out var channelIsLiveToken) || !bool.TryParse(channelIsLiveToken.ToString(), out var channelIsLive))
                throw new ArgumentException($"Wrong response. {channel}");

            return channelIsLive;
        }
        
        public static string GetUserIdFromChannelInfo(string responseContent)
        {
            var jObj = JObject.Parse(responseContent);

            if (!jObj.ContainsKey("result") || !(jObj["result"] is JObject result))
                throw new ArgumentException($"Wrong response. {jObj}");
            
            if (!result.ContainsKey("channel") || !(result["channel"] is JObject channel))
                throw new ArgumentException($"Wrong response. {result}");
            
            if (!channel.TryGetValue("user_id", out var userId))
                throw new ArgumentException($"Wrong response. {channel}");

            return userId.ToString();
        }
        public static IEnumerable<StreamInfo> TopStreamsParse(string responseContent)
        {
            var jObj = JObject.Parse(responseContent);

            if (!jObj.ContainsKey("result") || !(jObj["result"] is JArray results))
                throw new ArgumentException($"Wrong response. {jObj}");

            foreach (var resultToken in results)
            {
                if (!(resultToken is JObject result))
                    throw new ArgumentException($"Wrong response. {resultToken}");

                yield return GetStreamInfo(result);
            }
        }

        private static StreamInfo GetStreamInfo(JObject stream)
        {
            if (!stream.TryGetValue("media_container_name", out var title))
                throw new ArgumentException($"Wrong response. {stream}");

            if (!stream.TryGetValue("user_id", out var userId))
                throw new ArgumentException($"Wrong response {stream}");

            var previewImageUrl = $"https://cdn.wasd.tv/medium/live/{userId}/preview.jpg";

            if (!stream.ContainsKey("game") || !(stream["game"] is JObject game))
                throw new ArgumentException($"Wrong response {stream}");

            if (!game.TryGetValue("game_name", out var gameName))
                throw new ArgumentException($"Wrong response {game}");
            
            if (!stream.ContainsKey("media_container_streams") ||
                !(stream["media_container_streams"] is JArray mediaContainerStreams))
                throw new ArgumentException($"Wrong response {stream}");
            
            if (!(mediaContainerStreams.First is JObject mediaContainerStreamsElement))
                throw new ArgumentException($"Wrong response {mediaContainerStreams}");
            
            if (!mediaContainerStreamsElement.TryGetValue("stream_current_viewers", out var viewersCountToken) ||
                !int.TryParse(viewersCountToken.ToString(), out var viewersCount))
                
                throw new ArgumentException($"Wrong response {mediaContainerStreams}");

            var m3U8Url = $"https://cdn.wasd.tv/live/{userId}/index.m3u8";

            if (!stream.ContainsKey("media_container_channel") ||
                !(stream["media_container_channel"] is JObject mediaContainerChannel))
                throw new ArgumentException($"Wrong response {stream}");

            if (!mediaContainerChannel.TryGetValue("channel_name", out var name))
                throw new ArgumentException($"Wrong response {mediaContainerChannel}");

            return new StreamInfo(name.ToString(), title.ToString(), previewImageUrl, gameName.ToString(),
                viewersCount, m3U8Url);
        }
    }
}