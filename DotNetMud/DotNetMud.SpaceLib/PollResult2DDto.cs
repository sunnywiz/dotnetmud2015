using System;
using Newtonsoft.Json;

namespace DotNetMud.SpaceLib
{
    public class PollResult2DDto 
    {
        private const int RatePrecision = 3;
        private const int PositionalPrecision = 1;
        // uses decimal so that can be very specific on number of decimal places
        // using JsonProperty() on everything > 1 char 
        // using shortest string that is unique but related to field name
        // using uppercase!

        [JsonProperty("DR")]
        public decimal DR { get; set; }
        [JsonProperty("DX")]
        public decimal DX { get; set; }
        [JsonProperty("DY")]
        public decimal DY { get; set; }

        [JsonProperty("ID")]
        public long Id { get; set; }

        [JsonProperty("IM")]
        public string Image { get; set; }

        [JsonProperty("N")]
        public string Name { get; set; }

        [JsonProperty("R")]
        public decimal R { get; set; }

        [JsonProperty("RA")]
        public decimal Radius { get; set; }

        [JsonProperty("X")]
        public decimal X { get; set; }
        [JsonProperty("Y")]
        public decimal Y { get; set; }

        public PollResult2DDto(IObject2D t)
        {
            // Rounding things down to the precision the client needs
            X = Math.Round(Convert.ToDecimal(t.X),PositionalPrecision);
            Y = Math.Round(Convert.ToDecimal(t.Y),PositionalPrecision);
            DX = Math.Round(Convert.ToDecimal(t.DX),RatePrecision);
            DY = Math.Round(Convert.ToDecimal(t.DY),RatePrecision);
            Name = t.Name;
            Image = t.Image;
            DR = Math.Round(Convert.ToDecimal(t.DR),RatePrecision);
            R = Math.Round(Convert.ToDecimal(t.R),PositionalPrecision);
            Id = t.Id;
            Radius = Math.Round(Convert.ToDecimal(t.Radius),PositionalPrecision);
        }

        private PollResult2DDto()
        {
            
        }

        public PollResult2DDto CreateDiffFrom(PollResult2DDto p)
        {
            return new PollResult2DDto()
            {
                X = X,
                Y = Y,
                DX = DX,
                DY = DY,
                R = R,
                DR = DR,
                Radius = Radius,
                Name = (p.Name == Name ? null : Name),
                Image = (p.Image == Image ? null : Image)
            };
        }
    }
}