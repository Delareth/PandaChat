using CefSharp.Wpf;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace PandaChat
{
  public class MainController
  {
    public static ChromiumWebBrowser Browser { get; private set; }
    public static Settings Settings { get; private set; }
    public static TransparentMouse MouseController;

    private static MainWindow m_wInstance;

    public static void Init(MainWindow window)
    {
      LoadSettings();

      Settings.StreamerID = API.GetUserID(Settings.StreamerName);

      // вызываем асинхронный парсинг Badges для чата (важно, после загрузки настроек)
      BadgesData.Init();

      Log.Print("[System] Initializing browser...", LogTypes.INFO);

      //Create the browser
      Browser = EventSystem.RegisterEvents();

      Browser.LoadingStateChanged += (sender, args) =>
      {
        if (args.IsLoading == false)
        {
          // отключаем ПКМ по браузеру
          CallRawJS("document.oncontextmenu = function() { return false; };");

          Browser.PreviewMouseLeftButtonDown += WindowMouseDown;
          Browser.PreviewMouseLeftButtonUp += WindowMouseUp;
          Browser.SizeChanged += WindowSizeChanged;

          // иницализируем все, что связано с твичом и донатом
          Twitch.Init();
        }
      };

      window.MainGrid.Children.Add(Browser);

      // загружаем настройки в xaml
      m_wInstance = window;
      UpdateDataContext();

      API.IsChatLoaded = true;

      Log.Print("[System] Browser successful inited!", LogTypes.INFO);
    }

    public static void UpdateDataContext()
    {
      if (Thread.CurrentThread.Name == "Main")
      {
        m_wInstance.DataContext = null;
        m_wInstance.DataContext = Settings;

        return;
      }

      RunInMainThread(() =>
      {
        m_wInstance.DataContext = null;
        m_wInstance.DataContext = Settings;
      });
    }

    public static void CallJSFunction(string sFunction, params dynamic[] args)
    {
      string sParams = JsonConvert.SerializeObject(args);
      sParams = sParams.Replace("\\", "\\\\");
      sParams = sParams.Replace("'", "\\'");

      if (Thread.CurrentThread.Name == "Main")
      {
        Browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("invokeJS('" + sFunction + "', '" + sParams + "')");
        return;
      }

      RunInMainThread(() =>
      {
        Browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync("invokeJS('" + sFunction + "', '" + sParams + "')");
      });
    }

    public static void CallRawJS(string sCommand)
    {
      if (Thread.CurrentThread.Name == "Main")
      {
        Browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(sCommand);
        return;
      }

      RunInMainThread(() =>
      {
        Browser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(sCommand);
      });
    }

    private static void LoadSettings()
    {
      string sPath = Environment.CurrentDirectory + "/data/";

      if (Directory.Exists(sPath))
      {
        if (File.Exists(sPath + "cfg.json"))
        {
          Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(sPath + "cfg.json"));
          return;
        }
      }

      Settings = Settings.CreateSettings();
    }

    public static void RunInMainThread(Action a)
    {
      if (Browser.Dispatcher.CheckAccess()) a();
      else Browser.Dispatcher.BeginInvoke(a);
    }

    private static void WindowMouseDown(object sender, MouseButtonEventArgs e)
    {
      // если отключен клик по чату, то перетащить можно только с зажатым CTRL
      if (MouseController.AllowTransparency)
      {
        if (Keyboard.IsKeyDown(Key.LeftCtrl)) m_wInstance.DragMove();
      }
      else
      {
        if (Mouse.LeftButton == MouseButtonState.Pressed) m_wInstance.DragMove();
      }
    }

    private static void WindowMouseUp(object sender, MouseButtonEventArgs e)
    {
      double dLeft = Application.Current.MainWindow.Left;
      double dTop = Application.Current.MainWindow.Top;

      if (dLeft < 0) dLeft = 0;

      Settings.ChangePosition(dLeft, dTop);
    }

    private static void ResizingDone(SizeChangedEventArgs e)
    {
      Settings.ChangeSize(e.NewSize.Width, e.NewSize.Height);

      CallJSFunction("reloadSize");
    }

    private static void WindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
      Timers.Kill("WindowSizeChanged");

      Timers.Create("WindowSizeChanged", 750, false, () =>
      {
        ResizingDone(e);
      });
    }
  }
}
