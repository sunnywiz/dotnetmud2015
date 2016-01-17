using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetMud.A.Server;

namespace DotNetMud.A.MudLib
{
    /// <summary>
    /// Standard room object - rooms should inherit from here. 
    /// </summary>
    public class Room : StdObject, IProvideUserActions
    {
        private readonly Dictionary<string, string> _directionsToRooms = new Dictionary<string, string>(); 

        public string Description { get; set; }

        public Room()
        {
            Short = "A room.";
        }

        public override string Long
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Description);
                if (!_directionsToRooms.Any())
                {
                    sb.AppendLine("There are no exits.");
                }
                else
                {
                    sb.AppendFormat("There are {0} exits: ", _directionsToRooms.Count);
                    bool first = true; 
                    foreach (var dir in _directionsToRooms)
                    {
                        if (!first) sb.Append(", ");
                        sb.Append(dir.Key);
                    }
                    sb.AppendLine();
                }
                return sb.ToString();
            }
        }

        public void AddDirection(string direction, string roomLocator)
        {
            _directionsToRooms[direction] = roomLocator;
        }

        public IEnumerable<UserAction> GetUserActions()
        {
            foreach (var direction in _directionsToRooms)
            {
                yield return new UserAction() { Verb = direction.Key, Action = (uaec)=> { MoveUserToRoom(uaec); }};
            }
        }

        public void MoveUserToRoom(UserActionExecutionContext uaec)
        {
            string locator;
            if (_directionsToRooms.TryGetValue(uaec.Verb, out locator))
            {
                var targetRoom = Driver.Instance.FindSingletonByUri(locator);
                if (targetRoom != null)
                {
                    var userPreviousRoom = uaec.Player.Parent;
                    Driver.Instance.TellRoom(targetRoom,$"{uaec.Player.Short} arrives.");
                    // TODO: will have complications with messaging if move is blocked. need a better TellRoom which can exclude user.
                    uaec.Player.SendOutput($"You travel {uaec.Verb}");
                    uaec.Player.MoveTo(targetRoom);
                    uaec.Player.ReceiveInput("look");
                    if (userPreviousRoom != null) Driver.Instance.TellRoom(userPreviousRoom,$"{uaec.Player.Short} goes {uaec.Verb}.");
                }
            }
        }
    }
}