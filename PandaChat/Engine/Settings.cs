using Newtonsoft.Json;
using System;
using System.IO;

namespace PandaChat
{
  public class Settings
  {
    private bool m_bWindowOverlay;
    private bool m_bWindowTaskBar;

    private double m_dHeight;
    private double m_dWidth;
    private double m_dTop;
    private double m_dLeft;

    private string m_sShowInTaskBar;
    private string m_sMouseTransaprenty;
    private string m_sChangeSize;
    private string m_sResizeMode;
    private string m_sFirstInit;

    public string StreamerName { get; set; } = "";
    public string DonationAlertsToken { get; set; } = "";

    [JsonIgnore]
    public string StreamerID { get; set; }

    /// <summary>
    /// Уведомлять в чате о новых пользователях или ушедших
    /// </summary>
    public bool NotifyAboutUsers { get; set; } = true;

    public bool WindowOverlay
    {
      get
      {
        return m_bWindowOverlay;
      }
      set
      {
        if (m_bWindowOverlay == value) return;

        m_bWindowOverlay = value;

        SettingsChanded();
      }
    }

    public bool WindowTaskBar
    {
      get
      {
        return m_bWindowTaskBar;
      }
      set
      {
        if (m_bWindowTaskBar == value) return;

        m_bWindowTaskBar = value;

        SettingsChanded();
      }
    }

    public double Height
    {
      get
      {
        return m_dHeight;
      }
      set
      {
        if (m_dHeight == value) return;

        m_dHeight = value;
      }
    }

    public double Width
    {
      get
      {
        return m_dWidth;
      }
      set
      {
        if (m_dWidth == value) return;

        m_dWidth = value;
      }
    }

    public double Top
    {
      get
      {
        return m_dTop;
      }
      set
      {
        if (m_dTop == value) return;

        m_dTop = value;
      }
    }

    public double Left
    {
      get
      {
        return m_dLeft;
      }
      set
      {
        if (m_dLeft == value) return;

        m_dLeft = value;
      }
    }

    public string HeaderShowInTaskBar
    {
      get
      {
        return m_sShowInTaskBar;
      }
      set
      {
        if (m_sShowInTaskBar == value) return;

        m_sShowInTaskBar = value;

        SettingsChanded();
      }
    }

    public string HeaderMouseTransparent
    {
      get
      {
        return m_sMouseTransaprenty;
      }
      set
      {
        if (m_sMouseTransaprenty == value) return;

        m_sMouseTransaprenty = value;

        SettingsChanded();
      }
    }

    public string HeaderChangeSize
    {
      get
      {
        return m_sChangeSize;
      }
      set
      {
        if (m_sChangeSize == value) return;

        m_sChangeSize = value;

        SettingsChanded();
      }
    }

    public string ResizeMode
    {
      get
      {
        return m_sResizeMode;
      }
      set
      {
        if (m_sResizeMode == value) return;

        m_sResizeMode = value;

        SettingsChanded();
      }
    }

    public string FirstInit
    {
      get
      {
        return m_sFirstInit;
      }
      set
      {
        if (m_sFirstInit == value) return;

        m_sFirstInit = value;

        SettingsChanded();
      }
    }


    public static Settings CreateSettings()
    {
      Settings settings = new Settings
      {
        WindowOverlay = false,
        WindowTaskBar = true,
        Height = 300,
        Width = 300,
        Top = 0,
        Left = 0,
        HeaderShowInTaskBar = "Скрыть из панели задач",
        HeaderMouseTransparent = "Отключить кликабельность",
        HeaderChangeSize = "Отключить изменение размера",
        ResizeMode = "CanResizeWithGrip",
        FirstInit = "Visible",
        StreamerName = "scr13m",
        NotifyAboutUsers = true
      };

      settings.CreateNew();

      return settings;
    }

    public void Save()
    {
      using StreamWriter file = File.CreateText(Environment.CurrentDirectory + "/data/cfg.json");

      JsonSerializer serializer = new JsonSerializer();
      serializer.Serialize(file, this);

      file.Flush();
      file.Dispose();
    }

    public void CreateNew()
    {
      string sPath = Environment.CurrentDirectory + "/data/";

      Directory.CreateDirectory(sPath);

      Save();
    }

    private void SettingsChanded()
    {
      if (!API.IsChatLoaded) return;

      Save();
      MainController.UpdateDataContext();
    }

    /// <summary>
    /// Используем так, потому что в WPF width и height должно быть TwoWay
    /// </summary>
    public void ChangeSize(double dWidht, double dHeight)
    {
      Height = dHeight;
      Width = dWidht;

      SettingsChanded();
    }

    /// <summary>
    /// Используем так, потому что в WPF left и top должно быть TwoWay
    /// </summary>
    public void ChangePosition(double dLeft, double dTop)
    {
      Left = dLeft;
      Top = dTop;

      SettingsChanded();
    }
  }
}
