using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetMud.A.Server;

namespace DotNetMud.B.MudLib
{
    public class User : StdObject, IInteractive, IProvideUserActions
    {
        // TODO: provide a sane way for a monster to prevent a user from proceeding.  Might need monster override of direction
        private Dictionary<string, UserAction> _verbs;

        public void ReceiveInput(string line)
        {
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
                    SendOutput("unknown command.");
                }
            }

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

        public override void OnMoved(StdObject oldLocation, StdObject newLocation)
        {
            _verbs = null;
        }

        public void SendOutput(string text)
        {
            Driver<SampleGameSpecifics>.Instance.SendTextToPlayerObject(this, text);
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
                    var users = Driver<SampleGameSpecifics>.Instance.ListOfInteractives(); 
                    Driver<SampleGameSpecifics>.Instance.SendTextToPlayerObject(uaec.Player,$"There are {users.Length} users logged in:");
                    foreach (var user in users)
                    {
                        var x = user as User; 
                        if (x != null)
                        Driver<SampleGameSpecifics>.Instance.SendTextToPlayerObject(uaec.Player,$"  {x.Short}");
                    }
                }
            };
            yield return new UserAction()
            {
                Verb = "shout",
                Action = (uaec) =>
                {
                    var users = Driver<SampleGameSpecifics>.Instance.ListOfInteractives();
                    foreach (var user in users)
                    {
                        var x = user as User;
                        if (x != null)
                        {
                            if (x == uaec.Player)
                                Driver<SampleGameSpecifics>.Instance.SendTextToPlayerObject(uaec.Player,
                                    $"You shout: {String.Join(" ", uaec.Parameters)}");

                            else

                                Driver<SampleGameSpecifics>.Instance.SendTextToPlayerObject(x,
                                    $"{uaec.Player.Short} shouts: {String.Join(" ", uaec.Parameters)}");

                        }
                    }
                }
            };
        }

        private void DoSay(UserActionExecutionContext uaec)
        {
            // TODO: needs to add look at a player / object
            var parent = uaec.Player.Parent;
            if (parent == null)
            {
                Driver<SampleGameSpecifics>.Instance.SendTextToPlayerObject(uaec.Player, "Your words fall into the void.");
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
                        Driver<SampleGameSpecifics>.Instance.SendTextToPlayerObject(x, "You say: " + String.Join(" ", uaec.Parameters));
                    }
                    else
                    {
                        Driver<SampleGameSpecifics>.Instance.SendTextToPlayerObject(x,$"{uaec.Player.Short} says: {String.Join(" ",uaec.Parameters)}");
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
                Driver<SampleGameSpecifics>.Instance.SendTextToPlayerObject(uaec.Player,"You are hanging in the void.");
                return;
            }
            Driver<SampleGameSpecifics>.Instance.SendTextToPlayerObject(uaec.Player,parent.Short);
            Driver<SampleGameSpecifics>.Instance.SendTextToPlayerObject(uaec.Player,parent.Long);
            var first = true; 
            foreach (var obj in parent.GetInventory())
            {
                if (obj == uaec.Player) continue;
                if (first)
                {
                    Driver<SampleGameSpecifics>.Instance.SendTextToPlayerObject(uaec.Player, "Here you see: ");
                    first = false; 
                }
                Driver<SampleGameSpecifics>.Instance.SendTextToPlayerObject(uaec.Player, $"  {obj.Short}");
            }
            if (first) Driver<SampleGameSpecifics>.Instance.SendTextToPlayerObject(uaec.Player, "There is nothing to see here.");
        }

        public object RequestPoll(string pollName, object clientState)
        {
            if (pollName == "1")
            {
                var result = new PollResult1();
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
            else return null; 
        }
    }

    /// <summary>
    /// has to serialize down to json ok
    /// </summary>
    public class PollResult1
    {
        public PollResult1()
        {
            RoomInventory = new List<string>();
        }
        public string RoomDescription { get; set; }
        public List<string> RoomInventory { get; set; }
    }
}