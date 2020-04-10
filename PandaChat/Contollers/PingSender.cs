using System.Threading.Tasks;

namespace PandaChat
{
  public class PingSender
  {
    public static bool IsPingSuccessful = false;

    public static async void Start()
    {
      Log.Print("[System] PingSender started", LogTypes.INFO);

      // отправляем твичу пинг чтобы он не дисконнектил нас?
      Timers.Create("PingSender", 300000, true, () =>
      {
        Twitch.Irc.SendIrcMessage("PING irc.twitch.tv");

        // проверяем был ли пинг успешным
        Timers.Create("PingSender::CheckPingState", 10000, false, async () =>
        {
          if (IsPingSuccessful)
          {
            Log.Print("[System] Ping successful", LogTypes.INFO);
          }
          else
          {
            await Twitch.OnError("#3", "Can't respond ping state from twitch, reloading system");

            Twitch.Irc.Reload();
          }

          IsPingSuccessful = false;
        });
      });

      await Task.CompletedTask;
    }

    public static void Stop()
    {
      Timers.Kill("PingSender");
      Timers.Kill("PingSender::CheckPingState");

      IsPingSuccessful = false;
    }
  }
}
