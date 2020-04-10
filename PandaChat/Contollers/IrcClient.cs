using System;
using System.Net.Sockets;
using System.IO;

namespace PandaChat
{
  public class IrcClient
  {
    public string sUsername;
    private readonly string m_sChannel;
    private readonly string m_sIp;
    private readonly int m_iPort;
    private readonly string m_sPassword;

    private bool m_bIsReloading = false;

    private TcpClient m_tcpClient;
    private StreamReader m_inputStream;
    private StreamWriter m_outputStream;

    public IrcClient(string sIp, int iPort, string m_sUsername, string sPassword, string sChannel)
    {
      try
      {
        sUsername = m_sUsername;
        m_sChannel = sChannel;
        m_sIp = sIp;
        m_iPort = iPort;
        m_sPassword = sPassword;

        CreateTCP();
      }
      catch (Exception ex)
      {
        Log.Print(ex.Message);
      }
    }

    public void CreateTCP()
    {
      m_tcpClient = new TcpClient(m_sIp, m_iPort);
      m_inputStream = new StreamReader(m_tcpClient.GetStream());
      m_outputStream = new StreamWriter(m_tcpClient.GetStream());

      // Try to join the room
      m_outputStream.WriteLine("PASS " + m_sPassword);
      m_outputStream.WriteLine("NICK " + sUsername);
      m_outputStream.WriteLine("USER " + sUsername + " 8 * :" + sUsername);
      m_outputStream.WriteLine("JOIN #" + m_sChannel);
      m_outputStream.WriteLine("CAP REQ :twitch.tv/membership");
      m_outputStream.WriteLine("CAP REQ :twitch.tv/tags");
      m_outputStream.Flush();
    }

    public void Reload()
    {
      if (m_bIsReloading) return;

      m_bIsReloading = true;

      Disconnect();

      CreateTCP();

      Log.Print("[System] TCP Client reloaded", LogTypes.INFO);

      m_bIsReloading = false;
    }

    public void Disconnect()
    {
      m_tcpClient.GetStream().Flush();
      m_tcpClient.GetStream().Close();

      m_tcpClient.Close();
      m_tcpClient.Dispose();
    }

    public void SendIrcMessage(string message)
    {
      m_outputStream.WriteLine(message);
      m_outputStream.Flush();
    }

    public void SendChatMessage(string message)
    {
      SendIrcMessage(":" + sUsername + "!" + sUsername + "@" + sUsername + ".tmi.twitch.tv PRIVMSG #" + m_sChannel + " :" + message);
    }

    public string ReadMessage()
    {
      if (m_bIsReloading) return null;

      string message = m_inputStream.ReadLine();
      return message;
    }
  }
}
