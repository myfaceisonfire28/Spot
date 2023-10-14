using System.Diagnostics;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using static spot.Jsons;

namespace spot
{
    public class SpotifyManager
    {
        static Random Rand = new Random();
        static string? CurrentSong;
        static string? CurrentArtist;
        static string? PersonWhoAddedIt;
        static string? CurrentTrackId = "";


        //TODO make a lyrics (https://docs.genius.com/), playrandomsong, and seek command. Also make it log the songs that get played and who added them.
        //TODO just fix the code also add a read me and shit so people can actually use the program
        //TODO you know what you did for the C++ one with the rcon password and everything, do that again.

        static Root? QueueInfo = new Root();
        public static string ChatCommand = "say";

        ///every command
        static public List<string> Commands = new List<string> {
            "!add","!skip",
            "!back","!pause",
            "!play","!shuffle",
            "!current","!queue",
            "!random", "!rtd", 
            "!rps", "!8ball", 
            "!robot","!flip"
        }; 
        ///Responses to commands
        string[] CommandResponses = {
            "Queue adding", "",
            "","Paused music.",
            "Unpaused music.","Shuffle",
            "Currently playing: "+CurrentSong + " by " + CurrentArtist,"Say Queue in chat",
            "Not made","Not made", 
            "Rolled "+ Rand.Next(),EightBall[Rand.Next(EightBall.Length)], "",
            "Beep boop bop", Flip()
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

        static HttpClient httpClient = new HttpClient();

        static FormUrlEncodedContent formContent = new FormUrlEncodedContent(new[]{new KeyValuePair<string, string>("application/json"," ")});
        public static string Flip()
        {
            if(Rand.Next(100) >= 50)
            {
                return "Tails.";
            } 
            else
            {
                return "Heads.";
            }
        }

        public static string RPS(string Params)
        {
            switch(Rand.Next(1, 3))
            {
                case 1:
                    if(Params.ToLower().Contains("rock"))
                    {
                        return "Tie";
                    }
                    else if(Params.ToLower().Contains("scissors"))
                    {
                        return "Win";
                    }
                    else if(Params.ToLower().Contains("paper"))
                    {
                        return "Loss";
                    }
                return "Thats not a rock, paper, or scissors.";
                case 2:
                    if(Params.ToLower().Contains("scissors"))
                    {
                        return "Tie";
                    }
                    else if(Params.ToLower().Contains("paper"))
                    {
                        return "Win";
                    }
                    else if(Params.ToLower().Contains("rock"))
                    {
                        return "Loss";
                    }
                return "Thats not a rock, paper, or scissors.";
                case 3:
                    if(Params.ToLower().Contains("paper"))
                    {
                        return "Tie";
                    }
                    else if(Params.ToLower().Contains("rock"))
                    {
                        return "Win";
                    }
                    else if(Params.ToLower().Contains("scissors"))
                    {
                        return "Loss";
                    }
                return "Thats not a rock, paper, or scissors.";
                default:
                return "my code fucked up";
            }
        }

        public static void Init()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenManager.Token);
            GetCurrentSong().Wait();
            Queue.Clear();
        }


        static int SkipCounter = 0;
        static List<string> PeopleVoted = new List<string>();
        ///Manges the commands that get sent
        public static async Task<string> Manage(int CmdIndex, string PersonWhoCalled, string Params)
        {
            // Checks if the token is expired and then refreshes it
            if(DateTime.Now == TokenManager.ExpireTime){await TokenManager.RefreshTheToken();}

            Thread.Sleep(600);

            switch(CmdIndex)
            {   
                case 0: // Add song
                    return await AddToQueue(Params, PersonWhoCalled);
                case 1: // Skip song
                    if(SkipCounter >= 2 && !PeopleVoted.Contains(PersonWhoCalled))
                    {
                        SkipCounter = 0;
                        PeopleVoted.Clear();
                        return await SkipOrBack(true);
                    }
                    else if (!PeopleVoted.Contains(PersonWhoCalled))
                    {
                        SkipCounter++;
                        PeopleVoted.Add(PersonWhoCalled);
                        return SkipCounter+"/3 votes required to skip a song.";
                    }
                    else
                    {
                        return "Cant vote twice.";
                    }
                case 2: // Go back a song
                    return await SkipOrBack(false);
                case 3: // Pause song
                    return await PauseOrPlay(true);
                case 4: // Unpause song
                    return await PauseOrPlay(false);
                case 5: // Toggle shuffle
                    return await Shuffle();
                case 6: // Get current song
                    return $"Playing: {CurrentSong} by {CurrentArtist}. Added by {PersonWhoAddedIt}.";
                case 7: // Get current queue
                    return await GetQueue();
                case 8: // Plays a random song
                    return await RandomSong();
                case 9: // rtd
                    return $"Rolled: {Rand.Next()}";
                case 10: // rps
                    return RPS(Params);
                case 11: // 8ball
                    return EightBall[Rand.Next(EightBall.Length)];
                default:
                    return new SpotifyManager().CommandResponses[CmdIndex];
            }
        }


        static string characters = "abcdefghijklmnopqrstuvwxyz";        
        static async Task<string> RandomSong()
        {
            string RandomChar = characters[Rand.Next(25)].ToString();
              switch (Rand.Next(0, 1)) {
                case 0:
                RandomChar = RandomChar + '%';
                break;
                case 1:
                RandomChar = '%' + RandomChar + '%';
                break;
            }
            return await AddToQueue(RandomChar, "Mr random", Rand.Next(1000));
        }


        /// <summary>
        /// The queue of songs and who added them. <br/>
        /// The first item the arrays is the songs name <br/> 
        /// The second is the artist name <br/> 
        /// the third is the person who added it
        /// the forth is the spotify ID
        /// </summary>
        public static List<string[]> Queue = new List<string[]>();

        /// <summary>
        /// Adds a song to the queue.
        /// </summary>
        public static async Task<string> AddToQueue(string Song, string Person, int Offset = 0)
        {
            if(!Program.CanAdd) {return "Currently disabled.";}

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenManager.Token);
            var resp = await httpClient.GetAsync($"https://api.spotify.com/v1/search?q={Song}&type=track%2Cepisode&market=ES&offset={Offset}");
            
            Jsons.Root? model = JsonConvert.DeserializeObject<Jsons.Root>( await resp.Content.ReadAsStringAsync());


            try
            {
                httpClient.PostAsync("https://api.spotify.com/v1/me/player/queue?uri="+model.tracks.items[0].uri, formContent);

                if(Program.LogSongsAdded)
                {
                    File.AppendAllText("SongLog.txt", $"{Person} Added: {model.tracks.items[0].name} by {model.tracks.items[0].artists[0].name} \n");
                }
                Queue.Add(new string[] {model.tracks.items[0].name, model.tracks.items[0].artists[0].name, Person, model.tracks.items[0].id});
                return $"{Person} added: {model.tracks.items[0].name} by {model.tracks.items[0].artists[0].name} to my queue";
            }
            catch
            {
                return $"Failed to add: {Song}. sorry {Person}";
            }
        }


        /// <summary>
        /// Skips or goes back a song. <br/>
        /// <param name="Skip"> Controls if it skips or goes back as song</param>
        /// </summary>
        public static async Task<string> SkipOrBack(bool Skip)
        {
            if(Skip)
            {
                await httpClient.PostAsync("https://api.spotify.com/v1/me/player/next/", formContent);
            }
            else
            {
                await httpClient.PostAsync("https://api.spotify.com/v1/me/player/previous", formContent);
            }

            Thread.Sleep(100);
            await GetCurrentSong();
            return "";
        }

        /// <summary>
        /// Pauses or plays the music. <br/>
        /// <param name="Pause"> Controls if it pauses or plays the music</param>
        /// </summary>
        static async Task<string> PauseOrPlay(bool Pause)
        {
            if(Pause)
            {
                await httpClient.PutAsync("https://api.spotify.com/v1/me/player/pause/", formContent);
                return "Paused.";
            }
            else
            {
                await httpClient.PutAsync("https://api.spotify.com/v1/me/player/play", formContent);
                return "Unpaused.";
            }
        }


        static bool ShuffleState;
        static async Task<string> Shuffle()
        {
            await httpClient.PutAsync($"https://api.spotify.com/v1/me/player/shuffle?state={!ShuffleState}", formContent);
            ShuffleState = !ShuffleState;
            return $"Shuffle = {ShuffleState}";
        }

        /// <summary>
        /// Gets the current queue and says it in chat
        /// </summary>
        static public async Task<string>  GetQueue()
        {
            var resp = await httpClient.GetAsync("https://api.spotify.com/v1/me/player/queue?market=ES&limit=1");
            QueueInfo = JsonConvert.DeserializeObject<Jsons.Root>( await resp.Content.ReadAsStringAsync());
            string SendBack = "The Current Queue is: \n";
            
            for(int i = 0; i < 5; i++)
            {
                for(int y = 0; y < Queue.Count; y++)
                {
                    if(QueueInfo.queue[i].name == Queue[y][0])
                    {
                        SendBack += $"{QueueInfo.queue[i].name} by {QueueInfo.queue[i].artists[0].name}. Added by: {Queue[y][2]}. \n";
                        break;
                    }
                }
                if(Queue.Count == 0)
                {
                    SendBack += $"{QueueInfo.queue[i].name} by {QueueInfo.queue[i].artists[0].name}. \n";
                }
            }

            return SendBack+"And more.";
        }


        static int Prog = 0;
        static Stopwatch Stopwatch = new Stopwatch();

        /// <summary>
        /// Checks When the song changes
        /// </summary>
        public async Task SongCheck()
        {
            await GetCurrentSong();
            while(DateTime.Now < SongOverAt)
            {
                Thread.Sleep(100);
            }
            await SongCheck();
        }

        static DateTime SongOverAt;
        /// <summary>
        /// Gets the current song and says something in the tf2 chat
        /// </summary>
        static private async Task GetCurrentSong()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenManager.Token);


            if(DateTime.Now >= TokenManager.ExpireTime){await TokenManager.RefreshTheToken();}


            var response = await httpClient.GetAsync("https://api.spotify.com/v1/me/player/currently-playing?market=from_token");
            var jsonString = await response.Content.ReadAsStringAsync();
            Root? model = JsonConvert.DeserializeObject<Root>(jsonString);

            ShuffleState = model.shuffle_state;

            if(model.item.id != CurrentTrackId)
            {
                CurrentSong = model.item.name;
                CurrentTrackId = model.item.id;
                CurrentArtist = model.item.artists[0].name;

                try
                {
                    for(int i = 0; i < Queue.Count; i++)
                    {
                        if(Queue[i][3] == CurrentTrackId)
                        {
                            PersonWhoAddedIt = Queue[i][2];
                            Queue.RemoveAt(i);
                            break;
                        }
                    }

                }
                catch{}
                SkipCounter = 0;
                PeopleVoted.Clear();
                TF2Interface.SendCommand(ChatCommand, SendSongInChat(model.item.name, model.item.artists));
                PersonWhoAddedIt = null;
            }

            SongOverAt = DateTime.Now.AddMilliseconds(model.item.duration_ms - model.progress_ms);
        }

        /// <summary>
        /// Makes and returns a string of what will be sent in tf2 chat
        /// </summary>
        static private string SendSongInChat(string Name,List<Artist> Artists)
        {   
            string _artists = "";
            foreach(var artist in Artists)
            {
                _artists += artist.name + ", ";
            }


            if(PersonWhoAddedIt != null)
            {
                return $"Now playing: {Name} by {_artists} added by {PersonWhoAddedIt}";
            }
            return $"Now playing: {Name} by {_artists}";
        }
    }
}
