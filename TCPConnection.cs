﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace escript
{
    class TCPConnection
    {
        public static bool IsConnected = false;
        private static StreamWriter swSender;
        private static StreamReader srReceiver;
        private static TcpClient tcpServer;
        private static List<EMethod> currentMethods;
        private static Dictionary<string, int> currentLabels;

        public static int Connect(string IP, int PORT, List<EMethod> mth, Dictionary<string, int> lbls)
        {
            currentMethods = mth;
            currentLabels = lbls;
            try
            {
                if (IsConnected) return 0;

                tcpServer = new TcpClient();
                tcpServer.Connect(IP, PORT);
                IsConnected = true;
                new Thread(msg).Start();
                foreach (var m in currentMethods)
                {
                    if (m.Name == Variables.GetValue("TCP_triggerConnected"))
                    {
                        Cmd.Process(m.Name, currentMethods, currentLabels);
                    }
                }
                return 1;
            }
            catch { return -1; }
        }


        public static void Send(object text)
        {
            swSender = new StreamWriter(tcpServer.GetStream());
            swSender.WriteLine(text.ToString());
            swSender.Flush();
            //Llog("MW-Online", "To server: " + text.ToString());
        }
        public static void Disconnect()
        {
            IsConnected = false;
            foreach (var m in currentMethods)
            {
                if (m.Name == Variables.GetValue("TCP_triggerDisconnected"))
                {
                    Cmd.Process(m.Name, currentMethods, currentLabels);
                }
            }
            tcpServer.Close();
            try
            {
                swSender.Dispose();
            }
            catch { }
            try
            {
                srReceiver.Dispose();
            }
            catch { }
        }
        private static void msg()
        {
            try
            {
                srReceiver = new StreamReader(tcpServer.GetStream());
                while (true)
                {
                    string msg = srReceiver.ReadLine();
                    Variables.Set("tcpMsg", msg);
                    if (msg == null) throw new Exception("TCP says null");
                    foreach (var m in currentMethods)
                    {
                        if (m.Name == Variables.GetValue("TCP_triggerMsg"))
                        {
                            Cmd.Process(m.Name, currentMethods, currentLabels);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.ConWrLine("TCP ERROR: " + ex.Message); Disconnect();
            }
        }

    }
}
