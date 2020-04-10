using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PandaChat
{
  public class Utils
  {
    private static readonly Random random = new Random();

    public static bool ToInt(string sInput, out int iResult)
    {
      if (Int32.TryParse(sInput, out iResult))
      {
        return true;
      }

      return false;
    }

    public static bool ToInt(object oInput, out int iResult)
    {
      if (oInput is int)
      {
        if (ToInt(oInput.ToString(), out iResult))
        {
          return true;
        }
      }

      iResult = 0;
      return false;
    }

    public static bool ToInt(string sInput, Action<int> setValue)
    {
      if (ToInt(sInput, out int iValue))
      {
        setValue(iValue);
        return true;
      }
      return false;
    }

    public static bool ToInt(object oInput, Action<int> setValue)
    {
      if (ToInt(oInput, out int iValue))
      {
        setValue(iValue);
        return true;
      }
      return false;
    }

    public static int ToInt(object oInput)
    {
      ToInt(oInput, out int iResult);
      return iResult;
    }

    public static int ToInt(string sInput)
    {
      ToInt(sInput, out int iResult);
      return iResult;
    }

    /// <summary>
    /// Возвращает случайный элемент из массива
    /// </summary>
    public static T GetRandomElement<T>(IList<T> list)
    {
      return list[GetRandomInt(0, list.Count)];
    }

    /// <summary>
    /// Включает iMin, не включает iMax
    /// </summary>
    public static int GetRandomInt(int iMin, int iMax)
    {
      lock (random)
      {
        return random.Next(iMin, iMax);
      }
    }

    /// <summary>
    /// Включает dMin, не включает dMax
    /// </summary>
    public static double GetRandomDouble(double dMin = 0.0, double dMax = 1.0)
    {
      lock (random)
      {
        return random.NextDouble() * (dMax - dMin) + dMin;
      }
    }

    public static string FixTime(int iTime)
    {
      return (iTime < 10 ? "0" : "") + iTime;
    }

    public static async Task<string> GetContentFromUrl(string Url)
    {
      try
      {
        WebClient webclient = new WebClient
        {
          Encoding = Encoding.UTF8
        };

        string sContent = await webclient.DownloadStringTaskAsync(new Uri(Url));

        webclient.Dispose();

        return sContent.Replace('\r', ' ').Replace('\n', ' ');
      }
      catch (Exception ex)
      {
        Log.Print("Utils.GetContentFromUrl Exception", LogTypes.EXCEPTION);
        Log.Print(ex);

        return string.Empty;
      }
    }

    public static bool IsUrlValid(string sUrl)
    {
      return Uri.TryCreate(sUrl, UriKind.Absolute, out Uri uriResult)
        && (uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == Uri.UriSchemeHttp);
    }

    public static bool IsWebsiteValid(string sUrl)
    {
      try
      {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sUrl);
        request.GetResponse();

        return true;
      }
      catch
      {
        return false;
      }
    }

    public static DateTime ParseTwitchTime(string sDate)
    {
      string[] sRawTime = sDate.Split(' ');
      string[] sYear = sRawTime[0].Split('/');
      string[] sTime = sRawTime[1].Split(':');

      int iYear = ToInt(sYear[2]);
      int iMonth = ToInt(sYear[0]);
      int iDay = ToInt(sYear[1]);

      int iHour = ToInt(sTime[0]);
      int iMinute = ToInt(sTime[1]);
      int iSecond = ToInt(sTime[2]);

      return new DateTime(iYear, iMonth, iDay, iHour, iMinute, iSecond);
    }
  }
}
