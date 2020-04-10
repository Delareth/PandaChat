using CefSharp;
using CefSharp.Wpf;
using System;
using System.IO;

namespace PandaChat
{
  public static class Program
  {
    public static readonly bool IsDebug = false;

    /// <summary>
    /// Application Entry Point.
    /// </summary>
    [STAThread]
    public static int Main(string[] args)
    {
      //For Windows 7 and above, app.manifest entries will take precedences of this call
      Cef.EnableHighDPISupport();

      //We are using our current exe as the BrowserSubProcess
      //Multiple instances will be spawned to handle all the 
      //Chromium proceses, render, gpu, network, plugin, etc.
      var subProcessExe = new CefSharp.BrowserSubprocess.BrowserSubprocessExecutable();
      var result = subProcessExe.Main(args);
      if (result > 0)
      {
        return result;
      }

      if (IsDebug)
      {
        API.OpenConsole();

        Log.Init();
      }

      Timers.Init();

      //We use our current exe as the BrowserSubProcess
      var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

      var settings = new CefSettings()
      {
        //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
        CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
        BrowserSubprocessPath = exePath,
      };

      //Example of setting a command line argument
      //Enables WebRTC
      settings.CefCommandLineArgs.Add("enable-media-stream");
      if (IsDebug) settings.RemoteDebuggingPort = 8088;

      //Perform dependency check to make sure all relevant resources are in our output directory.
      Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

      var app = new App();
      app.InitializeComponent();
      return app.Run();
    }
  }
}
