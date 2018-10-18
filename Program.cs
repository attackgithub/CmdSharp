﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace escript
{
    class Program
    {
        public static ConsoleColor ScriptColor = ConsoleColor.White;
        static API api = null;
        public static string Out = "";
        public static int a = 0;
        public static void RunScript(string file)
        {

            StreamReader reader = new StreamReader(file);
            string[] fromfile = reader.ReadToEnd().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            reader.Close();

            reader.Dispose();

            Dictionary<string, int> Labels = new Dictionary<string, int>();
            List<EMethod> Methods = new List<EMethod>();

            for (int m = 0; m < fromfile.Length; m++)//methods
            {
                if (fromfile[m].StartsWith("func "))
                {

                    int methodStartIdx = 0;
                    if (fromfile[m + 1].StartsWith("{")) methodStartIdx = m + 2;
                    EMethod thisMethod = new EMethod();
                    thisMethod.Name = fromfile[m].Remove(0, "func ".Length);
                    int end = 0;
                    for (int c = methodStartIdx; c < fromfile.Length; c++)
                    {
                        if (!fromfile[c].StartsWith("}")) thisMethod.Code.Add(fromfile[c]);
                        else { end = c; break; }
                    }
                    m = end;
                    Methods.Add(thisMethod);
                }
            }

            for (int t = 0; t < fromfile.Length; t++)//labels
            {
                if (fromfile[t][0] == ':')
                {
                    Labels.Add(fromfile[t].Remove(0, 1), t);
                    //Program.ConWrLine("Added label: " + fromfile[t].Remove(0, 1) + ", line: " + t + 1);
                }

            }

            for (a = 0; a < fromfile.Length; a++)//code processing
            {
                string line = fromfile[a];
                if (line.StartsWith("func "))
                {
                    foreach (var m in Methods)
                    {
                        if (m.Name == line.Remove(0, "func ".Length))
                        {
                            a += (m.Code.Count + 2);
                        }
                    }
                }
                else
                {
                    if (Cmd.CmdParams["showCommands"] == "1") Program.ConWrLine(Cmd.CmdParams["invintation"] + line);
                    if (line.StartsWith("break")) break;

                    string result = Cmd.Process(Cmd.Str(line), Methods, Labels).ToString();
                    SetResult(result);
                    if (Cmd.CmdParams["showResult"] == "1") Program.ConWrLine(Cmd.CmdParams["result"]);
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        public static void ConWrLine(object text)
        {
            Console.WriteLine(text);
            if (api != null) api.trigger(text.ToString());
        }
        static void SetResult(string result)
        {
            Cmd.CmdParams["result"] = result;
        }
        public static void Init(string[] args, bool overwrite = true, API ap = null)
        {
            api = ap;
            if (overwrite)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Title = "ESCRIPT " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                //Program.ConWrLine(Console.Title);

            }
            
            Program.ConWrLine(" ");
            Program.ConWrLine(" = ESCRIPT by Dz3n | version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Program.ConWrLine(" = https://github.com/feel-the-dz3n/escript");
            Program.ConWrLine(" = https://vk.com/dz3n.escript");
            Program.ConWrLine(" = https://discord.gg/jXcjuqv");
            Program.ConWrLine(" ");
            
            if (overwrite)
            {
                Console.ForegroundColor = ScriptColor;
                Console.BackgroundColor = ConsoleColor.Black;

            }
#if DEBUG
            Debugger.Launch();
#endif
            Cmd.CmdParams.Clear();
            Cmd.CmdParams.Add("result", "-");
            Cmd.CmdParams.Add("splitArgs", "||");
            Cmd.CmdParams.Add("inputText", "ReadLine: ");
            Cmd.CmdParams.Add("showResult", "0");
            Cmd.CmdParams.Add("showCommands", "0");
            Cmd.CmdParams.Add("invintation", "ESCRIPT> ");


            
            if (overwrite)
            {
                try
                {
                    if (args.Length <= 0)
                    {
                        Cmd.CmdParams["showResult"] = "1";
                        while (true)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(Cmd.CmdParams["invintation"]);
                            Console.ForegroundColor = ConsoleColor.White;
                            string line = Console.ReadLine();
                            Console.ForegroundColor = ScriptColor;
                            if (line.StartsWith("break")) break;
                            SetResult(Cmd.Process(Cmd.Str(line), null, null));
                            if (Cmd.CmdParams["showResult"] == "1") Program.ConWrLine(Cmd.CmdParams["result"]);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                    }
                    else if (File.Exists(args[0]))
                    // if (File.Exists("D:\\vsprojects\\escript-master\\bin\\tcp_test.es"))
                    {
                        RunScript(args[0]);
                       // RunScript("D:\\vsprojects\\escript-master\\bin\\tcp_test.es"); 
                    }
                    else
                    {
                        if (args.Contains<string>("-close"))
                        {
                            foreach (var p in Process.GetProcesses())
                            {
                                if (p.ProcessName.ToLower() == "escript" && p.Id != Process.GetCurrentProcess().Id) p.Kill();
                            }
                        }
                        if(args.Contains<string>("-install"))
                        {
                            string InstallScript = "install.es";
                            using (StreamWriter w = new StreamWriter(InstallScript))
                            {
                                w.WriteLine("func InstallEscriptYes");
                                w.WriteLine("{");
                                w.WriteLine("InstallESCRIPT");
                                w.WriteLine("}");


                                w.WriteLine("func Canceled");
                                w.WriteLine("{");
                                w.WriteLine("Clear");
                                w.WriteLine("echo ");
                                w.WriteLine("SetColor 12");
                                w.WriteLine("echo ESCRIPT Installation");
                                w.WriteLine("echo =====================");
                                w.WriteLine("echo ");
                                w.WriteLine("SetColor 15");
                                w.WriteLine("echo The installation was canceled by user");
                                w.WriteLine("echo ");
                                w.WriteLine("}");


                                w.WriteLine("Clear");
                                w.WriteLine("echo ");
                                w.WriteLine("SetColor 10");
                                w.WriteLine("echo ESCRIPT Installation");
                                w.WriteLine("echo =====================");
                                w.WriteLine("SetColor 2");
                                w.WriteLine("echo * this installation is also script, but generated (install.es)");
                                
                                w.WriteLine("echo ");
                                w.WriteLine("echo ");
                                w.WriteLine("SetColor 15");
                                w.WriteLine("echo Welcome to ESCRIPT installation!");
                                w.WriteLine("echo ");
                                w.WriteLine("echo Do you want to install ESCRIPT?");
                                w.WriteLine("SetColor 7");
                                w.WriteLine("echo (admin rights will be requested)");
                                w.WriteLine("echo ");
                                w.WriteLine("SetColor 14");
                                w.WriteLine("set inputText y/n: ");
                                w.WriteLine("set install ^ReadLine");
                                w.WriteLine("if $install||==||y||InstallEscriptYes||Canceled");
                                
                            }
                            RunScript(InstallScript);
                            
                        }
                        if (args.Contains<string>("-assoc"))
                        {
                            Console.Clear();
                            Program.ConWrLine("Associating *.es files...");
                            FileAssociation.Associate("ESCRIPT file", "");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Program.ConWrLine("ESCRIPT was installed!");
                            Thread.Sleep(2000);
                            Cmd.CmdParams.Clear();
                            Main(new string[] { });
                        }
                        if (args.Contains<string>("-installNext"))
                        {
                            foreach (var p in Process.GetProcesses())
                            {
                                if (p.ProcessName.ToLower() == "escript" && p.Id != Process.GetCurrentProcess().Id) p.Kill();
                            }

                            string me = System.Reflection.Assembly.GetExecutingAssembly().Location;
                            string destination = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\ESCRIPT";
                            FileInfo aboutme = new FileInfo(me);

                            if (aboutme.DirectoryName == destination)
                            {
                                Program.ConWrLine("Download new version of ESCRIPT and start installer");
                            }
                            else
                            {

                                Program.ConWrLine("Installing ESCRIPT...");
                                if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\ESCRIPT")) Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\ESCRIPT");
                                File.Copy(me, destination + "\\" + aboutme.Name, true);
                                new Process() { StartInfo = { FileName = destination + "\\" + aboutme.Name, Arguments = "-close -assoc" } }.Start();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Program.ConWrLine(ex.ToString());
                    //if(args.Contains<string>("-install") && !args.Contains<string>("-close"))
                    //{
                    //    Program.ConWrLine("If you have some troubles with installation");
                    //    Program.ConWrLine("You should try to run ESCRIPT with -createInstallation argument");
                    //}
                }

                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("\nProgram stopped. Press 'R' to restart, 'D' for debug or any other key to exit ");
                    var key = Console.ReadKey().Key;

                    Program.ConWrLine("\n");
                    if (key == ConsoleKey.R) Main(args);
                    else if (key == ConsoleKey.D) Cmd.Process("set", null, null);
                    else Environment.Exit(0);
                }
            }
        }
        static void Main(string[] args)
        {
            Init(args);
        }
    }
}