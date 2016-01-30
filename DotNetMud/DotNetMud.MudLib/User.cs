using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetMud.Driver;

namespace DotNetMud.Mudlib
{
    public class User : MudLibObject, IProvideUserActions
    {
        // TODO: provide a sane way for a monster to prevent a user from proceeding.  Might need monster override of direction
        private Dictionary<string, UserAction> _verbs;

        private static readonly Dictionary<User, Action<string>> _registeredNextInputRedirects = new Dictionary<User, Action<string>>();

        public static Action<User, string> ServerSendsTextTOClientCallback { get; set; }

        public static Func<User[]> ListOfUsersCallback { get; set; }

        public static User[] ListOfUsers()
        {
            return ListOfUsersCallback?.Invoke(); 
        }

        public void ServerSendsTextToClient(string text)
        {
            ServerSendsTextTOClientCallback?.Invoke(this, text);
        }

        public void ClientSendsUserCommandToServer(string line)
        {
            Action<string> action;
            if (_registeredNextInputRedirects.TryGetValue(this, out action) && action != null)
            {
                _registeredNextInputRedirects.Remove(this);
                action(line);
                return;
            }

            if (String.IsNullOrWhiteSpace(line)) return;
            var split = line.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (split.Count > 0)
            {
                var verb = split[0];
                split.RemoveAt(0);
                if (String.IsNullOrWhiteSpace(verb)) return;
                verb = verb.Trim().ToLowerInvariant();

                if (_verbs == null)
                {
                    CollectVerbs();
                }

                UserAction userAction;
                if (_verbs.TryGetValue(verb, out userAction))
                {
                    var uaec = new UserActionExecutionContext()
                    {
                        Player = this,
                        Verb = verb,
                        UserAction = userAction,
                        Parameters = split
                    };
                    userAction.Action(uaec);
                }
                else
                {
                    ServerSendsTextToClient("unknown command.");
                }
            }

        }

        public PollResult ClientRequestsPollFromServer()
        {
            var result = new PollResult();
            var parent = this.Parent;
            if (parent == null)
            {
                result.RoomDescription = "You are floating in the void.";
            }
            else
            {
                var sb = new StringBuilder();
                sb.AppendLine(parent.Short);
                sb.AppendLine(parent.Long);
                result.RoomDescription = sb.ToString();

                foreach (var obj in parent.GetInventory())
                {
                    if (obj == this) continue;
                    result.RoomInventory.Add(obj.Short);
                }
            }
            return result;
        }

        public void PlayerGotDisconnected(bool wasItIntentional)
        {
            if (this.Parent != null) MudLibObject.TellRoom(this.Parent, $"{Short} vanishes in a puff of smoke.");
            GlobalObjects.RemoveStdObjectFromGame(this);
        }

        public void RedirectNextUserInput( Action<string> action)
        {
            _registeredNextInputRedirects[this] = action;
        }

        public void WelcomeNewPlayer()
        {
            ServerSendsTextToClient("Welcome to DotNetMud2015. ");
            ServerSendsTextToClient("");
            ServerSendsTextToClient("This is a sample mud library, more for setting up a driver and library");
            ServerSendsTextToClient("than for actual gaming.   ");
            ServerSendsTextToClient("");
            ServerSendsTextToClient("What name shall i know you by? ");

            var newPlayer = this; 

            RedirectNextUserInput((string text) =>
            {
                newPlayer.Short = text;
                newPlayer.ServerSendsTextToClient("Welcome, " + text);
                var room = GlobalObjects.FindSingleton(typeof(Lobby)) as MudLibObject;
                if (room != null)
                {
                    MudLibObject.TellRoom(room, $"{newPlayer.Short} arrives in a puff of smoke!");
                    newPlayer.MoveTo(room);
                }

                newPlayer.ClientSendsUserCommandToServer("look");
            });
        }

        public override void OnMoved(MudLibObject oldLocation, MudLibObject newLocation)
        {
            _verbs = null;
        }

        public IEnumerable<UserAction> GetUserActions()
        {
            yield return new UserAction()
            {
                Verb = "look",
                Action = (uaec) =>
                {
                    DoLook(uaec);
                }
            };
            yield return new UserAction()
            {
                Verb = "say",
                Action = (uaec) =>
                {
                    DoSay(uaec);
                }
            };
            yield return new UserAction()
            {
                Verb = "who",
                Action = (uaec) =>
                {
                    var users = ListOfUsers();
                    uaec.Player.ServerSendsTextToClient($"There are {users.Length} users logged in:");
                    foreach (var user in users)
                    {
                        var x = user as User;
                        if (x != null)
                            uaec.Player.ServerSendsTextToClient($"  {x.Short}");
                    }
                }
            };
            yield return new UserAction()
            {
                Verb = "shout",
                Action = (uaec) =>
                {
                    var users = User.ListOfUsers();
                    foreach (var user in users)
                    {
                        var x = user as User;
                        if (x != null)
                        {
                            if (x == uaec.Player)
                                uaec.Player.ServerSendsTextToClient($"You shout: {String.Join(" ", uaec.Parameters)}");
                            else
                                x.ServerSendsTextToClient($"{uaec.Player.Short} shouts: {String.Join(" ", uaec.Parameters)}");
                        }
                    }
                }
            };
        }

        private void CollectVerbs()
        {
            // go gather the verbs! 
            // things in your inventory override things in your environment override things in the room override things built in to you. 
            _verbs = new Dictionary<string, UserAction>();
            GetMoreVerbs(this);
            foreach (var ob in this.GetInventory())
            {
                var x = ob as IProvideUserActions;
                if (x != null)
                {
                    GetMoreVerbs(x);
                }
            }
            if (this.Parent != null)
            {
                foreach (var ob in this.Parent.GetInventory())
                {
                    var x = ob as IProvideUserActions;
                    if (x != null && x != this)
                    {
                        GetMoreVerbs(x);
                    }
                }
                var y = this.Parent as IProvideUserActions; 
                if (y != null) GetMoreVerbs(y);
            }
        }

        private void GetMoreVerbs(IProvideUserActions ob)
        {
            foreach (var action in ob.GetUserActions())
            {
                var key = action.Verb;
                if (String.IsNullOrEmpty(key)) continue;
                key = key.Trim().ToLowerInvariant();
                Console.WriteLine("GetMoreVerbs: registering verb {0} for player {1}",key,this.Short);
                _verbs[key] = action;
            }
        }

        private void DoSay(UserActionExecutionContext uaec)
        {
            // TODO: needs to add look at a player / object
            var parent = uaec.Player.Parent;
            if (parent == null)
            {
                uaec.Player.ServerSendsTextToClient("Your words fall into the void.");
                return; 
            }
            foreach (var ob in parent.GetInventory())
            {
                var x = ob as User;
                if (x != null)
                {
                    // TODO: should probably have a raw string rather than string.join of an array here. 
                    if (x == uaec.Player)
                    {
                        x.ServerSendsTextToClient("You say: " + String.Join(" ", uaec.Parameters));
                    }
                    else
                    {
                        x.ServerSendsTextToClient($"{uaec.Player.Short} says: {String.Join(" ",uaec.Parameters)}");
                    }
                }
            }
        }

        private void DoLook(UserActionExecutionContext uaec)
        {
            // TODO: needs to add look at a player / object
            var parent = uaec.Player.Parent;
            if (parent == null)
            {
                uaec.Player.ServerSendsTextToClient("You are hanging in the void.");
                return;
            }
            uaec.Player.ServerSendsTextToClient(parent.Short);
            uaec.Player.ServerSendsTextToClient(parent.Long);
            var first = true; 
            foreach (var obj in parent.GetInventory())
            {
                if (obj == uaec.Player) continue;
                if (first)
                {
                    uaec.Player.ServerSendsTextToClient("Here you see: ");
                    first = false; 
                }
                uaec.Player.ServerSendsTextToClient($"  {obj.Short}");
            }
            if (first) uaec.Player.ServerSendsTextToClient("There is nothing to see here.");
        }

        /// <summary>
        /// has to serialize down to json ok
        /// </summary>
        public class PollResult
        {
            public PollResult()
            {
                RoomInventory = new List<string>();
            }
            public string RoomDescription { get; set; }
            public List<string> RoomInventory { get; set; }
        }
    }
}