using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetMud.SpaceLib
{
    public class PollResult
    {
        public PollResult()
        {
            Others = new List<PollResult2DDto>();
        }

        [JsonProperty("A")]
        public PollResult2DDto Me { get; set; }
        [JsonProperty("SS")]
        public decimal ServerTimeInSeconds { get; set; }
        [JsonProperty("SR")]
        public decimal ServerTimeRate { get; set; }

        [JsonProperty("O")]
        public List<PollResult2DDto> Others { get; set; }
        [JsonProperty("H1")]
        public int MeHasBeenHitCount { get; set; }
        [JsonProperty("H2")]
        public int MeHasHitSomeoneCount { get; set; }
    }
}