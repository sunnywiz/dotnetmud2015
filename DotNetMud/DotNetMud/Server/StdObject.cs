using System.Collections.Generic;

namespace DotNetMud.Server
{
    /// <summary>
    /// This is the "standard mud object" that all mud objects should inherit from. 
    /// 
    /// </summary>
    public class StdObject
    {
        // every object has a short and long
        public string Short { get; set; }
        public string Long { get; set; } 
        // and every object has an ID
        public string URI { get; }

        private List<StdObject> _inventory;
        private StdObject _parentObject = null;
        public static long _idSequence = 0L;

        public override string ToString()
        {
            return URI;
        }

        public StdObject()
        {
            _inventory = new List<StdObject>();
            URI = this.GetType().FullName +'#'+ (++_idSequence);
        }

        public StdObject[] GetInventory()
        {
            return _inventory.ToArray(); 
        }

        public StdObject Parent
        {
            get
            {
                return _parentObject;
            }
        }

        public void MoveTo(StdObject target)
        {
            if (_parentObject != null)
            {
                _parentObject._inventory.Remove(this); 
            }
            if (target != null)
            {
                target._inventory.Add(this);
                this._parentObject = target;
            }
        }
    }
}