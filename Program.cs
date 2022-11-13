using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerTcpBS
{
    class Program
    {
        const string ip = "127.0.0.1";
        static TcpListener listener = new TcpListener(IPAddress.Parse(ip), 8080);
        static ConnectedClients clients;
        static ConnectedClients Tclients;
        static List<string> NickNames = new List<string>();
        static void Main(string[] args)
        {
            listener.Start();

            while (true)
            {
                var client = listener.AcceptTcpClient();
                Task.Factory.StartNew(() =>
                {
                    string nick="";
                  
                    StreamReader sr = new StreamReader(client.GetStream());
                    var sw = new StreamWriter(client.GetStream());
                    sw.AutoFlush = true;

                    while (client.Connected)
                    {
                        var line = sr.ReadLine();
                       
                        Console.WriteLine(line);
                        if(line.Contains("Connect:")&&!string.IsNullOrWhiteSpace(line.Replace("Connect:", "")))
                        {
                            nick = line.Replace("Connect:", "");
                            if(NickNames.FirstOrDefault(c=>c== nick) ==null)
                            {
                                NickNames.Add(nick);
                                sw.WriteLine("1");
                                break;

                            }
                            else
                            {
                                sw.WriteLine("0");
                                client.Client.Disconnect(false);
                            }
                        }
                    }
                   
                    while (client.Connected)
                    {
                        ConnectedClients pair = null;
                        sr = new StreamReader(client.GetStream());
                        sw = new StreamWriter(client.GetStream());
                        try
                        {

                            Console.WriteLine("Waiting for start");
                            var line = sr.ReadLine();
                            while(!line.Contains("Start"))
                            {
                                line = sr.ReadLine();
                            }
                            if (line.Contains("Tournament"))
                            {
                              
                                if (client.Connected)
                                {
                                    Console.WriteLine("Tgame start");
                                    if (Tclients == null)
                                    {
                                        Tclients = new ConnectedClients(client, nick);
                                        pair = Tclients;
                                        Console.WriteLine("First player start");
                                        while (pair.Player2 == null)
                                        {
                                            Task.Delay(50).Wait();
                                        }
                                    }
                                    else
                                    {
                                        Tclients.Player2 = client;
                                        Tclients.NickName2 = nick;
                                        pair = Tclients;
                                        Console.WriteLine("Second player start");
                                        Tclients = null;

                                    }
                                }
                            }
                            else
                            {
                                if (client.Connected)
                                {
                                    Console.WriteLine("Ugame start");
                                    if (client.Connected)
                                    {
                                        if (clients == null)
                                        {
                                            clients = new ConnectedClients(client, nick);
                                            pair = clients;
                                            Console.WriteLine("First player start");

                                            while (pair.Player2==null)
                                            {

                                                Task.Delay(50).Wait();
                                            }
                                        }
                                        else
                                        {

                                            clients.Player2 = client;
                                            clients.NickName2 = nick;
                                            pair = clients;
                                            clients = null;
                                            Console.WriteLine("Second player start");

                                        }
                                    }
                                }
                            }
                                WaitingForReady(sr, pair, nick);

                        }
                        catch { }
                        bool game = true;
                        Console.WriteLine("Game Start");
                        while (game)
                        {
                            try
                            {
                                var line = sr.ReadLine();

                                if (line=="e") break;

                                if (pair.Player2 != null)
                                {

                                    Console.WriteLine("Game "+line);

                                    if (pair.NickName1 == nick)
                                    {
                                        Send(line, pair, true);
                                    }
                                    else
                                    {
                                        Send(line, pair, false);
                                    }
                                }
                                else
                                {
                                    sw.WriteLine("00");
                                    client.Client.Disconnect(false);
                                }
                                if(line=="w")
                                WaitingForReady(sr, pair, nick);


                            }
                            catch { }
                        }
                        Console.WriteLine("Search "+nick);

                    }

                    NickNames.Remove(nick);
                });

            }
            
        }
        static async void Send(string str, ConnectedClients pair, bool first)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    TcpClient recipient;
                    if (first)
                    {
                        recipient = pair.Player2;
                    }
                    else
                    {
                        recipient = pair.Player1;
                    }

                    if (recipient.Connected)
                    {
                        var sw = new StreamWriter(recipient.GetStream());
                        sw.AutoFlush = true;

                        sw.WriteLine(str);
                    }
                    else
                    {
                        if (!first)
                            recipient = pair.Player1;
                        else
                            recipient = pair.Player2;

                        var sw = new StreamWriter(recipient.GetStream());

                        sw.WriteLine("000");
                        pair = null;
                        NickNames.Remove(pair.NickName1);
                        NickNames.Remove(pair.NickName2);
                    }
                }
                catch { }
            });
        }
        public static void WaitingForReady(StreamReader sr, ConnectedClients pair, string nick)
        {
            var line = sr.ReadLine();
            if (line.Contains("R"))
            {
                if (pair.NickName1 == nick)
                {
                    pair.Ready1 = true;
                    Send("R", pair, true);
                }
                else
                {
                    pair.Ready2 = true;
                    Send("R", pair, false);
                }
            }

            while (!pair.Ready1 || !pair.Ready2)
            {
                Task.Delay(50).Wait();
            }
            if (pair.FirstMove == "0")
            {
                Random rnd = new Random();
                int value = rnd.Next() % 2 + 1;
                if (value == 1)
                {
                    pair.FirstMove = "1";
                    Send("1", pair, false);
                    Send("2", pair, true);

                }
                else
                {
                    pair.FirstMove = "2";
                    Send("2", pair, false);
                    Send("1", pair, true);
                }
            }
            else
            {
                if(pair.FirstMove=="1")
                {

                    pair.FirstMove = "2";
                    Send("2", pair, false);
                    Send("1", pair, true);
                }
                else
                {
                    pair.FirstMove = "1";
                    Send("1", pair, false);
                    Send("2", pair, true);

                }

            }
        }
       
    }
   
}
