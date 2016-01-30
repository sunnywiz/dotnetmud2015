using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetMud.Driver
{
    /// <summary>
    /// This is what most mud code would know as the driver - the O/S of the mud, as it were. 
    /// </summary>
    public class GlobalObjects
    {
        private static List<StdObject> _allObjects;

        static GlobalObjects()
        {
            _allObjects = new List<StdObject>();
        }

        public static StdObject FindSingleton(Type objectType)
        {
            var alreadyExists = _allObjects.FirstOrDefault(x => x.GetType() == objectType);
            if (alreadyExists != null)
            {
                Console.WriteLine("FindSingleton(type): found {0}=>{1}", objectType.FullName, alreadyExists.ObjectId);
                return alreadyExists;
            }

            Console.WriteLine("FindSingleton(type): {0} not found, creating new", objectType.FullName);
            return CreateNewStdObject(objectType);
        }

        public static StdObject CreateNewStdObject(Type type)
        {
            if (type == null || !type.IsSubclassOf(typeof (StdObject))) return null;
            var ob = type.Assembly.CreateInstance(type.FullName, false) as StdObject;

            if (ob == null)
            {
                Console.WriteLine("CreateNewStdObject: {0} => created type was not StdObject, return null",
                    type.FullName);
                return null;
            }
            _allObjects.Add(ob);
            Console.WriteLine("CreateNewStdObject: {0} => created {1}", type.FullName, ob.ObjectId);
            return ob;
        }

        /// <summary>
        /// Do as much as I can to forget about an object. 
        /// </summary>
        /// <param name="ob"></param>
        public static void RemoveStdObjectFromGame(StdObject ob)
        {
            ob.Destroy();
            _allObjects.Remove(ob);
        }

    }
}