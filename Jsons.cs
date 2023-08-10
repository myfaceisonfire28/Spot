using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace spot
{
    public class Jsons
    {

        /// <summary>
        /// Info from the spotify api currently-playing
        /// </summary>
        public class Artist
        {
            public string? name { get; set; }
        }
        public class Item
        {
            public List<Artist>? artists { get; set; }
            public int duration_ms { get; set; }
            public string? id { get; set; }
            public string? uri { get; set; }
            public string? name { get; set; }
        }

        public class Root
        {
            public bool shuffle_state { get; set; }
            public int progress_ms { get; set; }
            public Item? item { get; set; }
            public Tracks? tracks { get; set; }
            public Episodes? episodes { get; set; }
            public List<Queue>? queue { get; set; }
        }

        /// <summary>
        /// Info from the spotify api search
        /// </summary>
        public class Episodes
        {
            public List<Item>? items { get; set; }
        }
        public class Tracks
        {
            public List<Item>? items { get; set; }
        }

        /// <summary>
        /// Info from the spotify api queue
        /// </summary>
        public class Queue
        {
            public List<Artist>? artists { get; set; }
            public string? name { get; set; }
        }

    }


}