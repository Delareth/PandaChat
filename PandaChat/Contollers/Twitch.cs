using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PandaChat
{
  public class Twitch
  {
    public static IrcClient Irc;

    private static int m_iErrorsCount = 0;
    private static Thread m_tChatThread;

    private static FollowersData m_FollowersData;

    private static SocketIO m_sDonation;

    public static void Init()
    {
      if (Irc != null)
      {
        Irc.Disconnect();
        m_tChatThread.Abort();
        PingSender.Stop();

        Timers.Kill("Twitch::SetStreamInfo");
        Timers.Kill("Twitch::Followers");

        m_sDonation.CloseAsync().GetAwaiter().GetResult();
        m_sDonation.Dispose();
      }

      if (MainController.Settings.StreamerName.Length == 0)
      {
        API.SendSystemMessage("Я не могу подключится потому что стример не указан");
        return;
      }

      Irc = new IrcClient("irc.twitch.tv", 6667, CFG.sBotName, CFG.sTwitchOAuth, MainController.Settings.StreamerName);
      PingSender.Start();

      SetStreamInfo();
      Timers.Create("Twitch::SetStreamInfo", 13000, true, SetStreamInfo);

      ConnectToDonate();
      InitFollowersCheck();

      // запускаем чтение твича
      m_tChatThread = new Thread(StartReadMessage);
      m_tChatThread.Start();

      API.SendSystemMessage("Я успешно подключился к чату " + MainController.Settings.StreamerName);
    }

    public static async void StartReadMessage()
    {
      try
      {
        string sMessage = Irc.ReadMessage();

        if (sMessage == null)
        {
          await OnError("#1", "IRC connection error, reloading system");

          Irc.Reload();
          return;
        }

        if (sMessage == ":tmi.twitch.tv PONG tmi.twitch.tv :irc.twitch.tv")
        {
          PingSender.IsPingSuccessful = true;
          return;
        }

        if (sMessage.Contains("JOIN"))
        {
          int iIndex = sMessage.IndexOf("!") - 1;
          string sNickname = sMessage.Remove(0, 1).Substring(0, iIndex);

          if (MainController.Settings.NotifyAboutUsers) API.UserJoinStream(sNickname);
          return;
        }

        if (sMessage.Contains("PART"))
        {
          int iIndex = sMessage.IndexOf("!") - 1;
          string sNickname = sMessage.Remove(0, 1).Substring(0, iIndex);

          if (MainController.Settings.NotifyAboutUsers) API.UserLeftStream(sNickname);
          return;
        }

        if (!sMessage.StartsWith("@")) return;

        if (!sMessage.Contains("PRIVMSG")) return;

        // обычные
        // @badge-info=;badges=vip/1;color=;display-name=mrvladislav134;emotes=;flags=; id=7e035af4-f2b5-4416-91b0-f1c2cb624031;
        // mod=0;room-id=263597313;subscriber=0;tmi-sent-ts=1585332691594;turbo=0;user-id=441434533;
        // user-type= :mrvladislav134!mrvladislav134@mrvladislav134.tmi.twitch.tv PRIVMSG #zhekaha :этот вопрос нужно было мне задать

        // выделенные
        // @badge-info=;badges=;color=;display-name=sanchop_;emotes=;flags=;id=d10765b9-8ec2-4263-8866-22c3961a84dc;
        // mod=0;msg-id=highlighted-message;room-id=213748641;subscriber=0;tmi-sent-ts=1585333272659;turbo=0;user-id=489801111;
        // user-type= :sanchop_!sanchop_ @sanchop_.tmi.twitch.tv PRIVMSG #csgomc_ru :подлизал режиссёру

        // @badge-info=;badges=;color=;display-name=pon1imayu;emotes=;flags=8-11:P.5;id=1bb84c57-5592-4015-841a-a699f2b049e0;
        // mod=0;msg-id=highlighted-message;room-id=213748641;subscriber=0;tmi-sent-ts=1585333668293;turbo=0;
        // user-id=495465557;user-type= :pon1imayu!pon1imayu @pon1imayu.tmi.twitch.tv PRIVMSG #csgomc_ru :Ууууууу сюка

        bool bIsHighlighted = sMessage.Contains("highlighted-message");

        // получаем префикс пользователя
        int iStartIndex = sMessage.IndexOf("badges=") + 7;
        int iEndIndex = sMessage.IndexOf(";", iStartIndex);

        string sBadges = sMessage[iStartIndex..iEndIndex];

        // получаем ник пользователя
        iStartIndex = sMessage.IndexOf("display-name=") + 13;
        iEndIndex = sMessage.IndexOf(";", iStartIndex);

        string sUsername = sMessage[iStartIndex..iEndIndex];

        // парсим сообщение
        string searchIndex = "#" + MainController.Settings.StreamerName + " :";
        int index = sMessage.IndexOf(searchIndex);
        sMessage = sMessage.Remove(0, index + searchIndex.Length);

        List<Badge> badges = BadgesData.GetBadgesFromString(sBadges);

        API.SendChatMessage(bIsHighlighted, badges, sUsername, sMessage);
      }
      catch (Exception ex)
      {
        await OnError("#2", ex.ToString());

        Irc.Reload();
      }
      finally
      {
        StartReadMessage();
      }
    }

    public static async Task OnError(string sCode, string sException)
    {
      Log.Print("[System] Exception with error code: " + sCode + " | " + m_iErrorsCount);
      Log.Print(sException, LogTypes.EXCEPTION);

      if (m_iErrorsCount > 5)
      {
        API.SendSystemMessage("Я не могу соединиться с твичом...");

        Timers.Create("ExitTimer", 5000, false, () =>
        {
          Environment.Exit(0);
        });
      }

      m_iErrorsCount++;

      Timers.Create("Error remove[" + m_iErrorsCount + "]", 5000, false, () =>
      {
        m_iErrorsCount--;
      });

      Task.Delay(3000).Wait();

      await Task.CompletedTask;
    }

    private static void SetStreamInfo()
    {
      ChannelInfo cInfo = API.GetChannelInfo();
      bool bIsOnline = cInfo != null;

      if (!bIsOnline)
      {
        API.SetStreamInfo(false, 0, 0);
        return;
      }

      int iFollowers = API.GetFollowersCount();

      API.SetStreamInfo(true, cInfo.Viewers, iFollowers);
    }

    public static void ConnectToDonate()
    {
      if (MainController.Settings.DonationAlertsToken.Length == 0)
      {
        API.SendSystemMessage("DonationAlerts отключен так как не указан token доната");
        return;
      }

      JObject json = new JObject
      {
        ["token"] = MainController.Settings.DonationAlertsToken,
        ["type"] = "minor"
      };

      m_sDonation = new SocketIO("ws://socket.donationalerts.ru/");

      // Listen server events
      m_sDonation.On("donation", res =>
      {
        try
        {
          var obj = JsonConvert.DeserializeObject(res.Text);
          dynamic nObj = JsonConvert.DeserializeObject(obj.ToString());

          if ((string)nObj["alert_type"] != "1") return;

          string sUsername = nObj["username"];
          string sMessage = nObj["message"];
          string sCurrency = nObj["currency"];
          int iAmount = nObj["amount"];
          bool bIsTestAlert = nObj["_is_test_alert"];

          if (bIsTestAlert) sUsername = "PandaChat";

          API.SendNewDonate(sUsername, sMessage, iAmount, sCurrency);
        }
        catch (Exception ex)
        {
          API.SendSystemMessage("Там донат пришел, но я не смог его обработать :(");
          
          Log.Print(ex.ToString());
        }
      });

      m_sDonation.OnConnected += async () =>
      {
        await m_sDonation.EmitAsync("add-user", JObject.FromObject(json));
      };

      m_sDonation.OnClosed += (obj) =>
      {
        Log.Print(obj);

        API.SendSystemMessage("Возникли проблемы с подключение к DonationAlerts");
        m_sDonation.Dispose();
      };

      m_sDonation.OnError += (obj) =>
      {
        Log.Print(obj);

        API.SendSystemMessage("Возникли проблемы с подключение к DonationAlerts");
        m_sDonation.Dispose();
      };

      // Connect to the server
      m_sDonation.ConnectAsync().GetAwaiter().GetResult();
    }

    public static void InitFollowersCheck()
    {
      m_FollowersData = API.GetLastFollowers(MainController.Settings.StreamerID, 40);

      Log.Print("Followers inited: " + m_FollowersData.Followers.Count, LogTypes.INFO);

      Timers.Create("Twitch::Followers", 3000, true, () =>
      {
        FollowersData fData = API.GetLastFollowers(MainController.Settings.StreamerID, 10);

        if (fData == null) return;

        List<Follower> newFollowers = new List<Follower>();

        foreach (Follower follower in fData.Followers)
        {
          if (m_FollowersData.Followers.Find(x => x.Name == follower.Name) == null)
          {
            m_FollowersData.AddNewFollower(follower);
            newFollowers.Add(follower);
          }
        }

        newFollowers.Reverse();

        foreach (Follower follower in newFollowers)
        {
          API.SendNewFollower(follower.Name);
        }
      });
    }
  }
}
