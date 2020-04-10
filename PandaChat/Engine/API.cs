using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PandaChat
{
  public class API
  {
    [DllImport("Kernel32")]
    public static extern void AllocConsole();

    [DllImport("Kernel32")]
    public static extern void FreeConsole();

    public static bool IsChatLoaded { get; set; } = false;

    public static void OpenConsole()
    {
      AllocConsole();
    }

    public static void CloseConsole()
    {
      FreeConsole();
    }

    public static void SendChatMessage(bool bIsHighlighted, List<Badge> lBadges, string sNickname, string sMsg)
    {
      MainController.CallJSFunction(bIsHighlighted ? "addHighlightedMessage" : "addMessage", lBadges, sNickname, sMsg);
    }

    public static void SendSystemMessage(string sMsg)
    {
      MainController.CallJSFunction("addMessage", "", "PandaChat", sMsg);
    }

    public static void SendNewFollower(string sNickname)
    {
      MainController.CallJSFunction("addFollower", sNickname);
    }

    public static void SendNewDonate(string sNickname, string sMsg, int iAmount, string sValue)
    {
      MainController.CallJSFunction("addDonate", sNickname, sMsg, iAmount, sValue);
    }

    public static void SetStreamInfo(bool bIsOnline, int iViewers, int iFollowers)
    {
      MainController.CallJSFunction("setStreamInfo", bIsOnline, iViewers, iFollowers);
    }

    public static void UserJoinStream(string sUsername)
    {
      MainController.CallJSFunction("userJoinStream", sUsername);
    }

    public static void UserLeftStream(string sUsername)
    {
      MainController.CallJSFunction("userLeftStream", sUsername);
    }

    public static string GetUserID(string sUsername)
    {
      string sResponse = GetFromTwitchAPI("https://api.twitch.tv/helix/users?login=" + sUsername.ToLower());

      dynamic dResult = JsonConvert.DeserializeObject<dynamic>(sResponse);

      if (dResult["data"].Count == 0) return null;

      return (string)dResult["data"][0]["id"];
    }

    public static int GetFollowersCount()
    {
      string sResponse = GetFromTwitchAPI("https://api.twitch.tv/helix/users/follows?to_id=" + MainController.Settings.StreamerID);

      dynamic dResult = JsonConvert.DeserializeObject<dynamic>(sResponse);

      if (dResult["total"] == null) return 0;

      return (int)dResult["total"];
    }

    public static ChannelInfo GetChannelInfo()
    {
      string sResponse = GetFromTwitchAPI("https://api.twitch.tv/helix/streams?user_login=" + MainController.Settings.StreamerName);

      dynamic dResult = JsonConvert.DeserializeObject<dynamic>(sResponse);

      if (dResult["data"].Count == 0) return null;

      return new ChannelInfo
      {
        Name = MainController.Settings.StreamerName,
        Title = dResult["data"][0]["title"],
        Viewers = (int)dResult["data"][0]["viewer_count"],
        StartedAt = Utils.ParseTwitchTime((string)dResult["data"][0]["started_at"])
      };
    }

    public static FollowersData GetLastFollowers(string sID, int iCount)
    {
      string sText = GetFromTwitchAPI("https://api.twitch.tv/helix/users/follows?first=" + iCount + "&to_id=" + sID);

      try
      {
        return JsonConvert.DeserializeObject<FollowersData>(sText);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        return null;
      }
    }

    // https://dev.twitch.tv/docs/api/reference
    public static string GetFromTwitchAPI(string sURL, bool bWithOAuth = true, bool bIsKrakenRequest = false)
    {
      RestClient client = new RestClient(new Uri(sURL));

      RestRequest request = new RestRequest(Method.GET);

      if (bIsKrakenRequest) request.AddHeader("Accept", "application/vnd.twitchtv.v5+json");

      request.AddHeader("Client-ID", CFG.sAppID);

      if (bWithOAuth) request.AddHeader("Authorization", "OAuth " + CFG.sTwitchOAuth.Split(':')[1]);

      request.RequestFormat = DataFormat.Json;
      IRestResponse response = client.Execute(request);

      return response.Content;
    }
  }
}
