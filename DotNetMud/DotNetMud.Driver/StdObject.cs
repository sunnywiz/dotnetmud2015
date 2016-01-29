namespace DotNetMud.Driver
{
    /// <summary>
    /// This is the "standard mud object" that all mud objects should inherit from. 
    /// 
    /// </summary>
    public class StdObject
    {
        // every object has a short and long

        // and every object has an ID

        // TODO: maybe make an internal constructor? 
        public string ObjectId { get; }  // set by driver on creation
        public string TypeUri { get; internal set; }
        public static long _idSequence = 0L;


        public override string ToString()
        {
            return ObjectId;
        }

        public StdObject()
        {
            ObjectId = this.GetType().FullName +'#'+ (++_idSequence);
            IsDestroyed = false;
        }

        public bool IsDestroyed { get; private set; }

        public virtual void Destroy()
        {
            IsDestroyed = true; 
        }
    }
}