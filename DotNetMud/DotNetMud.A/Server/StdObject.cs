using System.Collections.Generic;

namespace DotNetMud.A.Server
{
    /// <summary>
    /// This is the "standard mud object" that all mud objects should inherit from. 
    /// 
    /// </summary>
    public class StdObject
    {
        // every object has a short and long
        public string Short { get; set; }

        public virtual string Long => string.Empty;
        // and every object has an ID

        // TODO: maybe make an internal constructor? 
        public string ObjectId { get; }  // set by driver on creation
        public string TypeUri { get; internal set; }

        private readonly List<StdObject> _inventory;
        private StdObject _parentObject = null;
        public static long _idSequence = 0L;

        public override string ToString()
        {
            return ObjectId;
        }

        /// <summary>
        /// internal constructor - only driver (really) should be able to create a stdobject
        /// this is so we can do object tracking. 
        /// TODO: no longer internal when we split the thing out.   There needs to be DriverObject and a mudlib specific object, i think. 
        /// </summary>
        public StdObject()
        {
            _inventory = new List<StdObject>();
            ObjectId = this.GetType().FullName +'#'+ (++_idSequence);
            IsDestroyed = false;
        }

        public bool IsDestroyed { get; internal set; }

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

        public virtual void OnMoved(StdObject oldLocation, StdObject newLocation)
        {
            // override this to be told when you're moved. 
        }

    }
}