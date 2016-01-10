using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using DotNetMud.Server;

namespace DotNetMud.MudLib
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
                UserAction existingUa;
                if (_verbs.TryGetValue(key, out existingUa))
                {
                    if (existingUa.Priority > action.Priority) continue; // not overwriting
                }
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
            Driver.Instance.SendTextToPlayerObject(this, text);
        }

        public IEnumerable<UserAction> GetUserActions()
        {
            yield return new UserAction()
            {
                Verb = "look",
                Action = (uaec) =>
                {
                    DoLook(uaec);
                },
                Priority = 100
            };
            yield return new UserAction()
            {
                Verb = "say",
                Action = (uaec) =>
                {
                    DoSay(uaec);
                },
                Priority = 100
            };

        }

        private void DoSay(UserActionExecutionContext uaec)
        {
            // TODO: needs to add look at a player / object
            var parent = uaec.Player.Parent;
            if (parent == null)
            {
                Driver.Instance.SendTextToPlayerObject(uaec.Player, "Your words fall into the void.");
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
                        Driver.Instance.SendTextToPlayerObject(x, "You say: " + String.Join(" ", uaec.Parameters));
                    }
                    else
                    {
                        Driver.Instance.SendTextToPlayerObject(x,$"{uaec.Player.Short} says: {String.Join(" ",uaec.Parameters)}");
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
                Driver.Instance.SendTextToPlayerObject(uaec.Player,"You are hanging in the void.");
                return;
            }
            Driver.Instance.SendTextToPlayerObject(uaec.Player,parent.Short);
            Driver.Instance.SendTextToPlayerObject(uaec.Player,parent.Long);
            var first = true; 
            foreach (var obj in parent.GetInventory())
            {
                if (obj == uaec.Player) continue;
                if (first)
                {
                    Driver.Instance.SendTextToPlayerObject(uaec.Player, "Here you see: ");
                    first = false; 
                }
                Driver.Instance.SendTextToPlayerObject(uaec.Player, $"  {obj.Short}");
            }
            if (first) Driver.Instance.SendTextToPlayerObject(uaec.Player, "There is nothing to see here.");
        }
    }
}