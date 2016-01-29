using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetMud.Driver;

namespace DotNetMud.Mudlib
{
    /// <summary>
    /// Standard room object - rooms should inherit from here. 
    /// </summary>
    public class Room : MudLibObject, IProvideUserActions
    {
        private readonly Dictionary<string, Type> _directionsToRooms = new Dictionary<string, Type>(); 

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

        public void AddDirection(string direction, Type roomLocator)
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
            Type locator;
            if (_directionsToRooms.TryGetValue(uaec.Verb, out locator))
            {
                var targetRoom = Driver<SampleGameSpecifics>.Instance.FindSingleton(locator) as MudLibObject;
                if (targetRoom != null)
                {
                    var userPreviousRoom = uaec.Player.Parent;
                    MudLibObject.TellRoom(targetRoom,$"{uaec.Player.Short} arrives.");
                    // TODO: will have complications with messaging if move is blocked. need a better TellRoom which can exclude user.
                    uaec.Player.SendOutput($"You travel {uaec.Verb}");
                    uaec.Player.MoveTo(targetRoom);
                    // uaec.Player.ReceiveInput("look"); now done with Polling. 
                    if (userPreviousRoom != null) MudLibObject.TellRoom(userPreviousRoom,$"{uaec.Player.Short} goes {uaec.Verb}.");
                }
            }
        }
    }
}