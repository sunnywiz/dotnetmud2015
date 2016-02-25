using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetMud.Driver
{
    /// <summary>
    /// This is the "standard mud object" that all mud objects should inherit from. 
    /// 
    /// </summary>
    public class StdObject
    {

        // and every object has an ID

        // TODO: maybe make an internal constructor? 
        public string ReadableId { get; }  
        public long Id { get; }

        private static long _idSequence;

        private readonly List<StdObject> _inventory;
        private StdObject _parentObject;
        private static readonly object IdLock = new object();
        // TODO: maybe the StdObject.MoveLock could be made per object instead? 
        private static readonly object MoveLock = new object();

        public override string ToString()
        {
            return ReadableId;
        }

        public StdObject()
        {
            lock (IdLock)
            {
                Id = ++_idSequence;
                ReadableId = this.GetType().FullName + '#' + Id;
            }
            IsDestroyed = false;
            lock (MoveLock)
            {
                _inventory = new List<StdObject>();
                _parentObject = null;
            }
        }

        public bool IsDestroyed { get; private set; }

        public virtual void Destroy()
        {
            this.MoveTo(null); 
            IsDestroyed = true; 
        }

        public StdObject Parent
        {
            get
            {
                return _parentObject;
            }
        }

        public T[] GetInventory<T>()
        {
            lock (MoveLock)
            {
                // TODO: under load, the .ToArray() may not get through the collection while somebody else modifies it. 
                return _inventory.OfType<T>().ToArray();
            }
        }

        public void MoveTo(StdObject target)
        {
            lock (MoveLock)
            {
                var oldParentOb = _parentObject;
                if (_parentObject != null)
                {
                    _parentObject._inventory.Remove(this);
                    _parentObject.AfterExit(this);
                }
                if (target != null)
                {
                    target._inventory.Add(this);
                    this._parentObject = target;
                    target.AfterEnter(this);
                }
                this.AfterMoved(_parentObject, target);
            }
        }

        public virtual void AfterMoved(StdObject oldLocation, StdObject newLocation)
        {
            // override this to be told when you're moved. 
        }


        public virtual void AfterEnter(StdObject newChild)
        {
            // override this to deal with new children coming in
            // don't throw exceptions! 
        }

        public virtual void AfterExit(StdObject oldChild)
        {
            // override this to deal with old children coming in. 
        }
    }
}