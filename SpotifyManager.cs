using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using static spot.Jsons;

namespace spot
{
    public class SpotifyManager
    {
        static Random Rand = new Random();
        static string? CurrentPlaying;
        static string? CurrentArtist;

        //TODO make a lyrics, playrandomsong, and seek command. Also make it log the songs that get played and who added them.

        static Root? QueueInfo = new Root();
        public static string CommandName = "say";

        ///every command
        static public List<string> FullListOfCommands = new List<string> {
            "!add","!skip",
            "!back","!pause",
            "!unpause","!shuffle",
            "!current","!queue",
            "!rtd","!8ball",
            "!play", "!robot"
        }; 
        ///Responses to commands
        string[] CommandResponses = {
            "Queue adding", "",
            "","Paused music.",
            "Unpaused music.","Shuffle",
            "Currently playing: "+CurrentPlaying + " by " + CurrentArtist,"Say Queue in chat",
            "Rolled "+ Rand.Next(),EightBall[Rand.Next(EightBall.Length)], 
            "wait your turn and use ! add", "Beep boop bop"
        };

        ///EightBall responses
        static string[] EightBall = {
            "It is certain."," It is decidedly so.",
            "Without a doubt.","Yes definitely.",
            "You may rely on it.","As I see it, yes.",
            "Most likely.","Outlook good.",
            "Yes.","Signs point to yes.",
            "Reply hazy, try again.","Ask again later.",
            "better not tell you.","Cannot predict now.",
            "Concentrate and ask.","Don't count on it.",
            "My reply is no.","My sources say no.",
            "Outlook not so good.","Very doubtful.",
            "No wtf is wrong with you.",
            "IDK why are you asking me","Maybe",
            "Wait let me ask somebody !8ball"
        };
        ///API commands
        static public string[] ListOfApiCalls = {
            "https://api.spotify.com/v1/me/player/queue?uri=","https://api.spotify.com/v1/me/player/next/",
            "https://api.spotify.com/v1/me/player/previous/","https://api.spotify.com/v1/me/player/pause/",
            "https://api.spotify.com/v1/me/player/play","https://api.spotify.com/v1/me/player/shuffle?state=",
            "https://api.spotify.com/v1/me/player/pause/","https://api.spotify.com/v1/me/player/queue"
        };

        

        
        ///Manges the commands that get sent
        public async void Manage(string Command, string Song)
        {
            try{TokenManager.RefreshTheToken().Wait();}
            catch{}

            int IndexOfCommand = FullListOfCommands.IndexOf(Command);
            Thread.Sleep(600);
            if(Command == "!add")  {

                try
                {
                    string[] SearchResp = await SearchAndAdd(Song);

                    CommandResponses[0] = "added " + SearchResp[0]+" by "+ SearchResp[1] + " to my queue.";
                    HttpCall(IndexOfCommand,SearchResp[2]).Wait();


                }
                catch{TF2Interface.SendCommand(CommandName,"failed to add " + Song + ".");}

            }
            else if(Command=="!queue")
            {
                GetQueue();
            }
            else{HttpCall(IndexOfCommand,"").Wait();}
        }
        FormUrlEncodedContent formContent = new FormUrlEncodedContent(new[]{new KeyValuePair<string, string>("application/json"," ")});
        public async Task<string[]> SearchAndAdd(string Song)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenManager.Token);
            var resp = await client.GetAsync("https://api.spotify.com/v1/search?q="+Song+"&type=track%2Cepisode&market=ES&limit=1");
            Root? model = JsonConvert.DeserializeObject<Jsons.Root>( await resp.Content.ReadAsStringAsync());
            return new string[]{
                model.tracks.items[0].name,
                model.tracks.items[0].artists[0].name,
                model.tracks.items[0].uri};                     
        }

        bool ShuffleState;
        /// <summary>
        /// Calls the spotify API.
        /// </summary>
        private async Task HttpCall(int IndexOfCommand,string URI)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenManager.Token);

            
            if(IndexOfCommand == 5){
                var Playback = await TokenManager.Spotify.Player.GetCurrentPlayback(new PlayerCurrentPlaybackRequest());
                ShuffleState = Playback.ShuffleState;
                string APICall = ListOfApiCalls[5] + !ShuffleState;
                CommandResponses[5] = "Shuffle = " + !ShuffleState +".";
                HttpResponseMessage resp1 = await client.PutAsync(APICall,null);
                
            }
            else if(IndexOfCommand <= 5)
            {
                string APICall = ListOfApiCalls[IndexOfCommand] + URI;
                HttpResponseMessage resp = await client.PostAsync(APICall,formContent);
                HttpResponseMessage resp1 = await client.PutAsync(APICall,null);
            }

            if(IndexOfCommand == 1 || IndexOfCommand == 2){await GetCurrentSong();}
            else{TF2Interface.SendCommand(CommandName, CommandResponses[IndexOfCommand]);}
        }

        static public string GetQueue()
        {
            GetQueue2();
            return "";
        }
        static public async void GetQueue2()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenManager.Token);
            var resp = await client.GetAsync("https://api.spotify.com/v1/me/player/queue?market=ES&limit=1");
            QueueInfo = JsonConvert.DeserializeObject<Jsons.Root>( await resp.Content.ReadAsStringAsync());
            TF2Interface.SendCommand(CommandName,"The Current Queue is:");

            int AmountToSay = 5;
            if(QueueInfo.queue.ToArray().Length < 5){AmountToSay = QueueInfo.queue.ToArray().Length;}
            for(var i = 0; i<AmountToSay; i++)
            {
                Thread.Sleep(4000);
                try{
                    TF2Interface.SendCommand(CommandName,QueueInfo.queue[i].name + " by " +QueueInfo.queue[i].artists[0].name);
                }
                catch{}
            }
        }


        static int Prog = 0;
        static Stopwatch Stopwatch = new Stopwatch();

        /// <summary>
        /// Checks When the song changes
        /// </summary>
        public async Task SongCheck()
        {
            await GetCurrentSong();
            while(Stopwatch.ElapsedMilliseconds <= Prog){Thread.Sleep(100);}
            await SongCheck();         
        }

        static string? CurrentTrackId;
        private async Task GetCurrentSong()
        {
            try{await TokenManager.RefreshTheToken();}
            catch{}
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenManager.Token);
            var response = await client.GetAsync("https://api.spotify.com/v1/me/player/currently-playing?market=from_token");
            var jsonString = await response.Content.ReadAsStringAsync();
            Root? model = JsonConvert.DeserializeObject<Root>(jsonString);

            if(model.item.id != CurrentTrackId)
            {
                CurrentPlaying = model.item.name;
                CurrentTrackId = model.item.id;
                CurrentArtist = model.item.artists[0].name;
                TF2Interface.SendCommand(CommandName,SendSongInChat(model.item.name,model.item.artists));
            }
            Prog = model.item.duration_ms - model.progress_ms;
            Stopwatch.Restart();
        }

        /// <summary>
        /// Makes and returns a string of what will be sent in tf2 chat
        /// </summary>
        private string SendSongInChat(string Name,List<Artist> Artists)
        {   
            string _artists = " by ";
                foreach(var artist in Artists)
                {
                    _artists += artist.name + ", ";
                }
            
            if(Program.MicSpamming == false)
            {
                return "I'm now listening to: " + Name + _artists;
            }
            else
            {
                return "now playing: " + Name + _artists;
            }
        }
    }
}
