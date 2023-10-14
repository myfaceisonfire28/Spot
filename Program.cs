using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace spot
{
    /// TODO Message https://steamcommunity.com/id/pootpow after uploading it to github
    public class Program
    {
        public static string TF2Directory;
        public static string ClientID;
        public static bool ChatCommands = true;
        public static bool LogSongsAdded = false;
        public static bool LogCommands = false;
        public static bool CanAdd = true;
        public static bool TeamChat = false;

        static SpotifyManager Manager = new SpotifyManager();


        public static void Main()
        {

            if(!File.Exists("Settings.txt"))
            {
                Console.WriteLine("Ayo champ looks like you are missing your 'Settings.txt' file");
                PressAnyKey();
            }
            else
            {
                string[] FileLines = File.ReadAllLines("Settings.txt").ToArray();
                TF2Directory = FileLines[0];
                ClientID = FileLines[1];
                if(!Directory.Exists(TF2Directory))
                {
                    Console.WriteLine("your tf2 directory does not exist");
                    PressAnyKey();
                }
                else
                {
                    //Writes the to exec file so the program works.
                    List<string> AddThis = new List<string> {"con_logfile tf2consoleoutput.txt","ip 127.234.214.212","rcon_password pass","net_start","HasRanSpot"};
                    if(File.Exists(TF2Directory + "/cfg/Spot.cfg"))
                    {
                        var Autoexec = File.ReadAllLines(TF2Directory + "cfg/Spot.cfg").ToList();
                        Autoexec = Autoexec.Concat(AddThis).ToList();

                        File.WriteAllLines(TF2Directory + "cfg/Spot.cfg",Autoexec.AsEnumerable());
                    }
                    else
                    {
                        File.WriteAllLines(TF2Directory + "cfg/Spot.cfg",AddThis.AsEnumerable());
                    }
                }
                if(ClientID.Length != 32)
                {
                    Console.WriteLine("That clientID does not look right");
                    PressAnyKey();
                }
            }
            Console.Clear();
            TokenManager.GetToken();
        }


        ///Starts up everything
        public static void Start()
        {
            Task ConsoleUp = new Task(TF2Interface.CheckForConsoleUpdate);
            ConsoleUp.Start();
            Task SongUp = new Task(Manager.SongCheck().Wait);
            SongUp.Start();
            ConsoleControl();
        }

        ///Press any key to close
        private static void PressAnyKey()
        {
            Console.WriteLine("Press any key...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        /// Some controls for the user of the program
        static async void ConsoleControl()
        {
            while(true)
            {
                Console.Clear();
                Console.WriteLine("Stop: closes the program (you can also just press ctrl+C).");
                Console.WriteLine("Skip: The admin skip command.");
                Console.WriteLine("Commands: list the commands. \n");
                Console.WriteLine("Toggle: turns off/on commands.");
                Console.WriteLine($"Commands enabled = {ChatCommands}.\n");
                Console.WriteLine("Team: switches too and from team chat.");
                Console.WriteLine($"Team chat = {TeamChat}.\n");
                Console.WriteLine("Log: toggle for logging songs that get added to queue.");
                Console.WriteLine($"Log songs = {LogSongsAdded}.\n");
                Console.WriteLine("Add: turns off/on !add command.");
                Console.WriteLine($"Can add songs = {CanAdd}");

                switch(Console.ReadLine().ToLower())
                {
                    case "stop":
                        Environment.Exit(0);
                    break;
                    case "commands":
                        Console.Clear();
                        Console.WriteLine(@"!skip: skips the song.
                        !back: goes back a song (doesn't work on added songs).
                        !pause: pauses the song.
                        !play: unpauses the music.
                        !random: Adds a random song to queue.
                        !add [Song]: adds a song to queue.
                        !shuffle: toggles shuffle (doesn't work on added songs)
                        !current: says current song in chat.
                        !queue: says the next few songs in chat.
                        
                        Silly Commands
                        ==============
                        !rtd: rolls a random number.
                        !8ball: its an 8ball dumbass.
                        !robot: Beep boop bop.
                        !flip: Flip a coin.
                        !rps: Rock, paper, or scissors.

                        Press any key to stop looking....".Replace("    ",""));
                        Console.ReadKey();
                    break;
                    case "toggle":
                        ChatCommands = !ChatCommands;
                    break;
                    case "log":
                        LogSongsAdded = !LogSongsAdded;
                    break;
                    case "add":
                        CanAdd = !CanAdd;
                    break;
                    case "team":
                        TeamChat = !TeamChat;
                        if(!TeamChat){SpotifyManager.ChatCommand = "say";}
                        else{SpotifyManager.ChatCommand = "say_team";}
                    break;
                    case "skip":
                        SpotifyManager.SkipOrBack(true).Wait();
                    break;
                    case "command":
                        await SpotifyManager.Manage(SpotifyManager.Commands.IndexOf(Console.ReadLine()), "The DJ", Console.ReadLine());
                    break;
                }
            }
        }
    }
}
