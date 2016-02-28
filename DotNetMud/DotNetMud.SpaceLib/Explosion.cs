namespace DotNetMud.SpaceLib
{
    public class Explosion : StdObject2D
    {
        public Explosion()
        {
            Image = "http://i87.servimg.com/u/f87/12/97/11/39/small_10.gif";
            DR = 360; 
        }

        private System.Timers.Timer _expirationTimer; 

        public void StartExpirationTimer()
        {
            _expirationTimer = new System.Timers.Timer()
            {
                Interval = 1000.0, 
                AutoReset = false
            };
            _expirationTimer.Elapsed += (sender, args) => { this.Destroy(); };
            _expirationTimer.Start(); 
        }
    }
}