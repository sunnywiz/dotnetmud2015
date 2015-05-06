using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace BareMud1
{
    class Program
    {
        public static Room StartRoom; 

        static void Main(string[] args)
        {
            // Do some stuff to bring the mud up and bring up a player object
            Console.WriteLine("type 'quit' to exit");

            StartRoom = new Room(); 

            string url = "http://localhost:8080";
            using (WebApp.Start(url))
            {
                Console.WriteLine("Server running on {0}", url);
                Console.ReadLine();
            }
        }
    }
}
