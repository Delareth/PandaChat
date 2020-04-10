using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PandaChat
{
  public class FollowersData
  {
    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("data")]
    public List<Follower> Followers { get; set; }

    [JsonIgnore]
    public int NewFollowersCount { get; private set; }

    public void AddNewFollower(Follower follower)
    {
      NewFollowersCount++;

      Followers.Add(follower);
    }
  }

  public class Follower
  {
    [JsonProperty("from_id")]
    public string ID { get; set; }
    [JsonProperty("from_name")]
    public string Name { get; set; }
    [JsonProperty("followed_at")]
    public string FollowedAt { get; set; }
  }

  public class ChannelInfo
  {
    public string Name { get; set; }
    public string Title { get; set; }
    public int Viewers { get; set; }
    public DateTime StartedAt { get; set; }
  }
}
