using System;
using System.Threading;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace PandaChat
{
  public partial class MainWindow : Window
  {
    private readonly TaskbarIcon m_tIcon;

    public MainWindow()
    {
      Thread.CurrentThread.Name = "Main";

      InitializeComponent();

      MainController.Init(this);

      m_tIcon = (TaskbarIcon)FindName("WpfTaskIcon");
    }

    #region MainControl

    private void WindowSourceInitialized(object sender, EventArgs e)
    {
      MainController.MouseController = new TransparentMouse(this)
      {
        AllowTransparency = MainController.Settings.WindowOverlay
      };
    }

    private void WindowClosed(object sender, EventArgs e)
    {
      CloseButton(sender, e);
    }

    #endregion

    #region TreyControl

    private void FirstMousePress(object sender, EventArgs e)
    {
      if (MainController.Settings.FirstInit == "Visible")
      {
        MainController.Settings.FirstInit = "Hidden";
      }
    }

    private void CloseButton(object sender, EventArgs e)
    {
      MainController.Settings.Save();

      m_tIcon.ContextMenu.Dispatcher.DisableProcessing();
      m_tIcon.Dispose();

      Environment.Exit(0);
    }

    private void SettingsButton(object sender, EventArgs e)
    {
      Log.Print("Call settings");
    }

    private void MouseButton(object sender, EventArgs e)
    {
      MainController.MouseController.AllowTransparency = !MainController.MouseController.AllowTransparency;
      MainController.Settings.WindowOverlay = MainController.MouseController.AllowTransparency;

      if (MainController.MouseController.AllowTransparency)
      {
        MainController.Settings.HeaderMouseTransparent = "Включить кликабельность";
      }
      else
      {
        MainController.Settings.HeaderMouseTransparent = "Отключить кликабельность";
      }
    }

    private void ChangeSizeButton(object sender, EventArgs e)
    {
      if (MainController.Settings.ResizeMode == "NoResize")
      {
        MainController.Settings.ResizeMode = "CanResizeWithGrip";
        MainController.Settings.HeaderChangeSize = "Отключить изменение размера";
      }
      else
      {
        MainController.Settings.ResizeMode = "NoResize";
        MainController.Settings.HeaderChangeSize = "Включить изменение размера";
      }
    }

    private void ShowInToolbarButton(object sender, EventArgs e)
    {
      MainController.Settings.WindowTaskBar = !MainController.Settings.WindowTaskBar;

      if (MainController.Settings.WindowTaskBar)
      {
        MainController.Settings.HeaderShowInTaskBar = "Скрыть из панели задач";
      }
      else
      {
        MainController.Settings.HeaderShowInTaskBar = "Отображать в панели задач";
      }
    }

    #endregion

    #region TestButtons
    private void TestMessageButton(object sender, EventArgs e)
    {
      for (int i = 0; i < 13; i++)
      {
        API.SendSystemMessage("Я есть панда!");
      }
    }

    private void TestFollowerButton(object sender, EventArgs e)
    {
      API.SendNewFollower("pandaaaa");
    }

    private void TestDonateButton(object sender, EventArgs e)
    {
      API.SendNewDonate("pandaa", "Ты лучший!", 50, "RUB");
    }
    #endregion
  }
}
