using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetMud.MudLib;
using Microsoft.Owin.Hosting;

namespace DotNetMud
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://localhost:8080";
            using (WebApp.Start(url))
            {
                Console.WriteLine("Server running on {0}", url);
                Console.ReadLine();
            }
        }
    }
}
