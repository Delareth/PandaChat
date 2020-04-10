using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace PandaChat
{
  public class Badge
  {
    public string Title { get; set; }
    public string Image { get; set; }

    public Badge(string sTitle, string sImage)
    {
      Title = sTitle;
      Image = sImage;
    }
  }

  public class BadgesData
  {
    // название сета > название версии, дата
    private static readonly Dictionary<string, Dictionary<string, Badge>> m_dBadges = new Dictionary<string, Dictionary<string, Badge>>();

    public static async void Init()
    {
      string sBadges = await Utils.GetContentFromUrl("https://badges.twitch.tv/v1/badges/global/display");

      dynamic dObject = JsonConvert.DeserializeObject(sBadges);

      foreach (var i in (JObject)dObject["badge_sets"])
      {
        string sBadgeSetName = i.Key;

        m_dBadges.Add(sBadgeSetName, new Dictionary<string, Badge>());

        foreach (var x in (JObject)dObject["badge_sets"][sBadgeSetName]["versions"])
        {
          JToken value = x.Value;

          m_dBadges[sBadgeSetName].Add(x.Key, new Badge((string)x.Value["title"], (string)x.Value["image_url_2x"]));
        }
      }

      InitLocalBadges();
    }

    private static void InitLocalBadges()
    {
      int iGlobalBadges = m_dBadges.Count;

      string sBadges = API.GetFromTwitchAPI("https://badges.twitch.tv/v1/badges/channels/" + MainController.Settings.StreamerID  +"/display");

      dynamic dObject = JsonConvert.DeserializeObject(sBadges);

      foreach (var i in (JObject)dObject["badge_sets"])
      {
        string sBadgeSetName = i.Key;

        // это значит, что у некоторых сетов перезаписаны картинки или титлы
        if (m_dBadges.ContainsKey(sBadgeSetName))
        {
          m_dBadges.Remove(sBadgeSetName);
          iGlobalBadges--;
        }

        m_dBadges.Add(sBadgeSetName, new Dictionary<string, Badge>());

        foreach (var x in (JObject)dObject["badge_sets"][sBadgeSetName]["versions"])
        {
          JToken value = x.Value;

          m_dBadges[sBadgeSetName].Add(x.Key, new Badge((string)x.Value["title"], (string)x.Value["image_url_2x"]));
        }
      }

      Log.Print("[System] Added global badges: " + iGlobalBadges, LogTypes.INFO);
      Log.Print("[System] Added channel badges: " + (m_dBadges.Count - iGlobalBadges), LogTypes.INFO);
      Log.Print("[System] Total loaded badges: " + m_dBadges.Count, LogTypes.INFO);
    }

    public static Badge GetBadge(string sBadgeSet, string sBadgeVersion)
    {
      if (!m_dBadges.ContainsKey(sBadgeSet)) return null;

      if (!m_dBadges[sBadgeSet].ContainsKey(sBadgeVersion)) return null;

      return m_dBadges[sBadgeSet][sBadgeVersion];
    }

    public static List<Badge> GetBadgesFromString(string sBadges)
    {
      List<Badge> badges = new List<Badge>();

      string[] sBadgesMassive = sBadges.Split(",");

      foreach (string sBadge in sBadgesMassive)
      {
        string[] sBadgeInfo = sBadge.Split("/");

        if (sBadgeInfo.Length != 2) continue;

        badges.Add(GetBadge(sBadgeInfo[0], sBadgeInfo[1]));
      }

      return badges;
    }
  }
}
