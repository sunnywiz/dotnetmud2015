using System;
using System.Linq;
using System.Threading.Tasks;

namespace BareMud1
{
    class Program
    {
        static void Main(string[] args)
        {
            // Do some stuff to bring the mud up and bring up a player object
            Console.WriteLine("type 'quit' to exit");

            var room = new Room();
            var userObject = new User();
            (userObject as IInteractive).RegisterOutputStream(Console.Out);
            userObject.MoveTo(room);
            
            userObject.ReceiveInput("look");

            string line; 
            while ((line = Console.ReadLine()) != "quit")
            {
                Task.Factory.StartNew(() =>
                    {
                        userObject.ReceiveInput(line);
                    });
            }
        }
    }
}
