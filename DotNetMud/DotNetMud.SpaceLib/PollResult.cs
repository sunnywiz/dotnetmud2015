using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DotNetMud.SpaceLib
{
    public class PollResult
    {
        public PollResult()
        {
            Others = new Dictionary<long,PollResult2DDto>();
        }

        [JsonProperty("A")]
        public PollResult2DDto Me { get; set; }
        [JsonProperty("SS")]
        public decimal ServerTimeInSeconds { get; set; }
        [JsonProperty("SR")]
        public decimal ServerTimeRate { get; set; }

        [JsonProperty("O")]
        public Dictionary<long,PollResult2DDto> Others { get; set; }
        [JsonProperty("H1")]
        public int MeHasBeenHitCount { get; set; }
        [JsonProperty("H2")]
        public int MeHasHitSomeoneCount { get; set; }

        /// <summary>
        /// This is to reduce network traffic of things that don't change much. 
        /// Note that nulls get sent as :null, so if something has less than 5 chars, probably not
        /// worth spending the time to abbreviate it.   Also things that change a lot, 
        /// probalby not worth it either. 
        /// </summary>
        /// <param name="p">what we know of the state that client should already have</param>
        /// <returns>what we're going to send instead of this.</returns>
        public PollResult CreateDiffFrom(PollResult p)
        {
            var result = new PollResult()
            {
                Me = Me.CreateDiffFrom(p.Me),
                ServerTimeInSeconds = ServerTimeInSeconds,
                ServerTimeRate = ServerTimeRate,
                Others = Others.ToDictionary(x=>x.Key, x=>x.Value),
                MeHasBeenHitCount = MeHasBeenHitCount,
                MeHasHitSomeoneCount = MeHasHitSomeoneCount
            };
            foreach (var mk in Others.Keys)
            {
                PollResult2DDto po;
                if (p.Others.TryGetValue(mk, out po))
                {
                    // we had a previous version of this thing
                    result.Others[mk] = Others[mk].CreateDiffFrom(po);
                }
                else
                {
                    // didn't have a previous version.  No abbreviation
                }
            }
            // if previous.Others had additional keys that we didn't have this time, they disappear. 
            return result; 
        }
    }
}