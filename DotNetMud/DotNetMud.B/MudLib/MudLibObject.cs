using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetMud.Driver;

namespace DotNetMud.B.MudLib
{
    public class MudLibObject : StdObject
    {
        private List<MudLibObject> _inventory;
        private MudLibObject _parentObject = null;
        public string Short { get; set; }
        public virtual string Long => string.Empty;

        public MudLibObject()
        {
            _inventory = new List<MudLibObject>();
        }

        public MudLibObject Parent
        {
            get
            {
                return _parentObject;
            }
        }

        public MudLibObject[] GetInventory()
        {
            return _inventory.ToArray(); 
        }

        public void MoveTo(MudLibObject target)
        {
            // TODO: it feels like inventory, movement, etc - are all mudlib specific things, rather than Driver things.
            var oldParentOb = _parentObject; 
            if (_parentObject != null)
            {
                _parentObject._inventory.Remove(this); 
            }
            if (target != null)
            {
                target._inventory.Add(this);
                this._parentObject = target;
            }
            this.OnMoved(oldParentOb, target);
        }

        public virtual void OnMoved(MudLibObject oldLocation, MudLibObject newLocation)
        {
            // override this to be told when you're moved. 
        }

        public override void Destroy()
        {
            this.MoveTo(null);
            base.Destroy();

        }

        /// <summary>
        /// Tell all the interactives (people who can receive messages) in a room something. 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="message"></param>
        public static void TellRoom(MudLibObject room, string message)
        {
            if (room != null && !String.IsNullOrEmpty(message))
            {
                foreach (var ob in room.GetInventory())
                {
                    var ob2 = ob as IInteractive;
                    if (ob2 != null)
                    {
                        ob2.SendOutput(message);
                    }
                }
            }
        }

    }
}
