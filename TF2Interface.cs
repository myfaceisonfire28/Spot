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


        static public void ReadConsole(object sender, FileSystemEventArgs e)
        {
            if(Program.ChatCommands == false){return;}
            SpotifyManager Manager = new SpotifyManager();
            string LastLine = File.ReadLines(Program.TF2Directory + "/tf2consoleoutput.txt").Last();
            string Command = " ";
            
            foreach (string IsXThere in SpotifyManager.FullListOfCommands)//Checks the last line if the console and if/what command is there
            {
                if(LastLine.ToLower().Contains(IsXThere))
                {
                    Command = IsXThere;
                }
            }

            if(Command == "!add")
            {
                string Song = LastLine.Substring(LastLine.IndexOf("!") + 4);
                foreach(string X in SpotifyManager.FullListOfCommands){Song.Replace(X,"");}
                Manager.Manage(Command,Song);
            }
            else if(SpotifyManager.FullListOfCommands.Contains(Command))
            {
                Manager.Manage(Command, " ");
            } 
        }


        static RconClient Client = RconClient.Create("127.234.214.212",27015);
        static bool Auth;
        /// <summary>
        /// Sends commands to tf2 using RCON
        /// </summary>
        static public async void SendCommand(string Command,string Params)
        {
            if(Auth)
            {
                await Client.ExecuteCommandAsync(Command + " " + Params);
            }
            else
            {
                await Client.ConnectAsync();   
                Auth = await Client.AuthenticateAsync("pass");
                await Client.ExecuteCommandAsync(Command + " " + Params);
            }
        }
    }
}