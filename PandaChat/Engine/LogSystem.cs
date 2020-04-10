using System;
using System.IO;

namespace PandaChat
{
  public enum LogTypes
  {
    INFO,
    MSG,
    ERROR,
    WARN,
    FILESYSTEM,
    EXCEPTION,
    DEFAULT
  }

  public static class Log
  {
    private static StreamWriter m_StreamWriter;

    private static string m_sLogFilePath;

    public static void UnhandledExceptionsHandler(object sender, UnhandledExceptionEventArgs args)
    {
      Exception ex = (Exception)args.ExceptionObject;
      Print(ex);
    }

    public static bool Init(string sFilePath = "")
    {
      AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionsHandler);

      DateTime tempDate = DateTime.Now;
      string sCurrentDate = Utils.FixTime(tempDate.Day) + "." + Utils.FixTime(tempDate.Month) + "." + tempDate.Year;

      if (sFilePath.Length == 0)
      {
        string sPackage = Directory.GetCurrentDirectory() + "/logs";

        if (!Directory.Exists(sPackage))
        {
          Directory.CreateDirectory(sPackage);
        }

        sFilePath = sPackage + "/[" + sCurrentDate + "]_log.txt";
      }

      m_sLogFilePath = sFilePath;

      try
      {
        m_StreamWriter = new StreamWriter(m_sLogFilePath, true)
        {
          AutoFlush = false
        };

        m_StreamWriter.WriteLine("");
        m_StreamWriter.WriteLine("-----------------");
        m_StreamWriter.WriteLine("");
        m_StreamWriter.WriteLine("PandaChat v" + CFG.sVersion + " started at: " +
          Utils.FixTime(tempDate.Hour) + ":" + Utils.FixTime(tempDate.Minute) + ":" + Utils.FixTime(tempDate.Second));
        m_StreamWriter.Flush();

        Print("[Logs] System initialized!", LogTypes.INFO);
        Print("[Logs] ----------------------------", LogTypes.INFO);
        return true;
      }
      catch (Exception ex)
      {
        Print("[Logs] System initializing failed!", LogTypes.INFO);
        Print("[Logs] ----------------------------", LogTypes.INFO);
        Print(ex.ToString(), LogTypes.ERROR);
        return false;
        throw ex;
      }
    }

    public static void Print(dynamic obj, LogTypes logType = LogTypes.DEFAULT)
    {
      Print(obj.ToString(), logType);
    }

    public static void Print(string sInput, LogTypes logType = LogTypes.DEFAULT)
    {
      if (!Program.IsDebug) return;

      ConsoleColor prefixColor = ConsoleColor.White;
      ConsoleColor textColor = ConsoleColor.White;

      string sPrefix = "";

      DateTime tempDate = DateTime.Now;
      string sTimeStamp = Utils.FixTime(tempDate.Hour) + ":" + Utils.FixTime(tempDate.Minute) + ":" + Utils.FixTime(tempDate.Second);

      switch (logType)
      {
        case LogTypes.DEFAULT:
        {
          prefixColor = ConsoleColor.White;
          textColor = ConsoleColor.White;
          sPrefix = "";
          break;
        }
        case LogTypes.INFO:
        {
          prefixColor = ConsoleColor.Green;
          textColor = ConsoleColor.White;
          sPrefix = "[INFO]";
          break;
        }
        case LogTypes.MSG:
        {
          prefixColor = ConsoleColor.Blue;
          textColor = ConsoleColor.White;
          sPrefix = "[MSG]";
          break;
        }
        case LogTypes.ERROR:
        {
          prefixColor = ConsoleColor.Red;
          textColor = ConsoleColor.Red;
          sPrefix = "[ERROR]";
          break;
        }
        case LogTypes.WARN:
        {
          prefixColor = ConsoleColor.Yellow;
          textColor = ConsoleColor.Yellow;
          sPrefix = "[WARN" +
              "]";
          break;
        }
        case LogTypes.EXCEPTION:
        {
          prefixColor = ConsoleColor.Red;
          textColor = ConsoleColor.Magenta;
          sPrefix = "[EXCEPTION]";
          break;
        }
        case LogTypes.FILESYSTEM:
        {
          prefixColor = ConsoleColor.Red;
          textColor = ConsoleColor.Cyan;
          sPrefix = "[FILESYSTEM]";
          break;
        }
      }

      string sTimePrefix = "[" + sTimeStamp + "]" + sPrefix + " ";

      if (m_StreamWriter != null)
      {
        m_StreamWriter.Write(sTimePrefix);
        m_StreamWriter.WriteLine(sInput);
        m_StreamWriter.Flush();
      }

      Console.ForegroundColor = prefixColor;
      Console.Write(sTimePrefix);

      Console.ForegroundColor = textColor;
      Console.WriteLine(sInput);

      Console.ResetColor();
    }

    public static void Print(Exception ex)
    {
      Print(ex.ToString(), LogTypes.EXCEPTION);
    }
  }
}
