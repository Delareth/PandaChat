using CefSharp;
using CefSharp.Wpf;
using System;

namespace PandaChat
{
  public class EventSystem
  {
    public static ChromiumWebBrowser RegisterEvents()
    {
      ChromiumWebBrowser Browser = new ChromiumWebBrowser(Environment.CurrentDirectory + "./ui/index.html");

      // У нас SPA, поэтому можно юзать старый способ
      CefSharpSettings.LegacyJavascriptBindingEnabled = true;

      // Регистрируем класс ивентов для взаимодействия с C# через JS
      Browser.JavascriptObjectRepository.Register("ptrigger", new Events(), isAsync: true, options: BindingOptions.DefaultBinder);

      return Browser;
    }

    // Ивенты которые можно юзать в JS через ptrigger (функция в JS должна быть с маленькой буквы)
    private class Events
    {
      //public void BrowserReady()
      //{
      //  API.SendChatMessage("PandaChat", "API успешно подключено");
      //}
    }
  }
}
