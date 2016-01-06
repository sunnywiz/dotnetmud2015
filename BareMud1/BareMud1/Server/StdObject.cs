using System;
using System.Collections.Generic;

namespace BareMud1
{
    public class StdObject
    {
        // every object has a short and long
        public string Short { get; set; }
        public string Long { get; set; } 
        // and every object has an ID
        public string Id { get; private set; }

        private List<StdObject> _inventory;
        private StdObject _parentObject = null;
        public static long _idSequence = 0L; 

        public StdObject()
        {
            _inventory = new List<StdObject>();
            Id = this.GetType().Name + (++_idSequence);
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