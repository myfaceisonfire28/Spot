using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RconSharp;

namespace spot
{
    public class TF2Interface
    {
        ///Starts up an event & event handler for each time the console is updated
        static public void CheckForConsoleUpdate()
        {
            using var watcher = new FileSystemWatcher(Program.TF2Directory);
            watcher.NotifyFilter = NotifyFilters.Size;
            watcher.Changed += ReadConsole;
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
            while(true) {}
        } 

        
        static public async void ReadConsole(object sender, FileSystemEventArgs e)
        {
            if(Program.ChatCommands == false){return;}

            string LastLine = File.ReadLines(Program.TF2Directory+"tf2consoleoutput.txt").Last();
            if(!LastLine.Contains("!") && !LastLine.Contains(":"))
            {
                return;
            }

            if(Program.TeamChat && !LastLine.Contains(("team")))
            {
                return;
            }



            int i = 0;
            foreach (string IsXThere in SpotifyManager.Commands)
            {
                if(LastLine.ToLower().Contains(IsXThere))
                {
                    var PersonWhoCalled = LastLine.Substring(0, LastLine.IndexOf(":")).Replace("*DEAD*","");
                    LastLine = LastLine.Substring(LastLine.IndexOf(":")+1);
                    
                    var Params = LastLine.Substring(LastLine.IndexOf(IsXThere) + IsXThere.Length);
                    SendCommand("say", await SpotifyManager.Manage(i, PersonWhoCalled, Params));
                    return;
                }
                i++;
            }
        }


        static RconClient Client = RconClient.Create("127.234.214.212",27015);
        static bool Auth;
        /// <summary>
        /// Sends commands to tf2 using RCON
        /// </summary>
        static public async void SendCommand(string Command,string Params)
        {
            if(!Auth)
            {
                await Client.ConnectAsync();   
                Auth = await Client.AuthenticateAsync("pass");
            }

        
            string[] Split = Params.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            
            if(Split.Length == 1)
            {
                Thread.Sleep(1000);
                await Client.ExecuteCommandAsync($"{Command} {Split[0]}");
                return;
            }
            
            foreach(string str in Split)
            {
                Thread.Sleep(4000);
                await Client.ExecuteCommandAsync($"{Command} {str}");
            }

        }
    }



}