using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace PandaChat
{
  public static class Timers
  {
    private static Dictionary<string, Timer> m_Timers;

    public static bool Init()
    {
      try
      {
        m_Timers = new Dictionary<string, Timer>();

        Log.Print("[Timers] System initialized!", LogTypes.INFO);

        return true;
      }
      catch (Exception ex)
      {
        Log.Print("[Timers] System initializing failed!", LogTypes.WARN);

        return false;
        throw ex;
      }
    }

    public static bool Create(string sTimerName, uint uiIntervalMS, bool bRepeat, Action action)
    {
      if (m_Timers.ContainsKey(sTimerName))
      {
        return false;
      }

      Timer callingTimer = new Timer(uiIntervalMS);

      m_Timers.Add(sTimerName, callingTimer);
      callingTimer.AutoReset = bRepeat;
      callingTimer.Elapsed += async (sender, e) => await Task.Run(action);

      if (!bRepeat)
      {
        callingTimer.Elapsed += delegate
        {
          Kill(sTimerName);
        };
      }
      callingTimer.Enabled = true;

      return true;
    }

    public static bool Kill(string sTimerName)
    {
      Timer timer = GetTimer(sTimerName);

      if (timer == null)
      {
        return false;
      }

      timer.Stop();
      timer.Close();

      return m_Timers.Remove(sTimerName);
    }

    public static Timer GetTimer(string sTimerName)
    {
      if (!m_Timers.ContainsKey(sTimerName))
      {
        return null;
      }

      return m_Timers[sTimerName];
    }
  }
}