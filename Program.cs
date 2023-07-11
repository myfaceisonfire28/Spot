using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace spot
{

    public class Program
    {
        public static string TF2Directory;
        public static string ClientID;
        public static bool ChatCommands = true;
        public static bool MicSpamming = true;
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
                    //Writes the to autoexec file so the program works.
                    List<string> AddThis = new List<string> {"con_logfile tf2consoleoutput.txt","ip 127.234.214.212","rcon_password pass","net_start","HasRanSpot"};
                    if(File.Exists(TF2Directory + "/cfg/Autoexec.cfg")&!File.ReadAllLines(TF2Directory + "cfg/Autoexec.cfg").Contains("HasRanSpot"))
                    {
                        var Autoexec = File.ReadAllLines(TF2Directory + "cfg/Autoexec.cfg").ToList();
                        Autoexec = Autoexec.Concat(AddThis).ToList();

                        File.WriteAllLines(TF2Directory + "cfg/Autoexec.cfg",Autoexec.AsEnumerable());
                    }
                    else
                    {
                        File.WriteAllLines(TF2Directory + "cfg/Autoexec.cfg",AddThis.AsEnumerable());
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




        ///Press any key and close
        private static void PressAnyKey()
        {
                Console.WriteLine("Press any key...");
                Console.ReadKey();
                Environment.Exit(0);
        }


        /// Some controls for the user of the program
        static void ConsoleControl()
        {
            Console.Clear();
            Console.WriteLine("Stop: closes the program (you can also just press ctrl+C).");
            Console.WriteLine("Commands: list the commands. \n");
            Console.WriteLine("Toggle: turns off/on commands.");
            Console.WriteLine("Commands enabled = " + ChatCommands+ ".\n");
            Console.WriteLine("Spam: changes from 'im listening to to' to 'now playing'.");
            Console.WriteLine("Mic spamming = " + MicSpamming + ".\n");
            Console.WriteLine("Team: switches two and from team chat.");
            Console.WriteLine("Team chat = " + TeamChat+".");
            switch(Console.ReadLine().ToLower())
            {
                case "stop":
                    Environment.Exit(0);
                break;
                case "commands":
                    Console.Clear();
                    Console.WriteLine("!skip: skips the song.");
                    Console.WriteLine("!back: goes back a song.");
                    Console.WriteLine("!pause: pauses the song.");
                    Console.WriteLine("!unpause: unpauses the music.");
                    Console.WriteLine("!add [Song]: adds a song to queue.");
                    Console.WriteLine("!shuffle: toggles shuffle.");
                    Console.WriteLine("!current: says current song in chat.");
                    Console.WriteLine("!queue: says the next few songs in chat.");
                    Console.WriteLine("!rtd: rolls a random number.");
                    Console.WriteLine("!8ball: its an 8ball dumbass.");
                    Console.WriteLine("!robot: Beep boop bop.");
                    Console.WriteLine("Press any key to stop looking....");
                    Console.ReadKey();
                break;
                case "toggle":
                    ChatCommands = !ChatCommands;
                break;
                case "team":
                    TeamChat = !TeamChat;
                    if(!TeamChat){SpotifyManager.CommandName = "say";}
                    else{SpotifyManager.CommandName = "say_team";}
                break;
                case "spam":
                    MicSpamming = !MicSpamming;
                break;
                case "command":
                    Manager.Manage(Console.ReadLine()," ");
                break;
            }
            ConsoleControl();
        }
    }
}
