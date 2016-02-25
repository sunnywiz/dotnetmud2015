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
        private readonly static List<StdObject> AllObjects;

        static GlobalObjects()
        {
            AllObjects = new List<StdObject>();
        }

        public static StdObject FindSingleton(Type objectType)
        {
            var alreadyExists = AllObjects.FirstOrDefault(x => x.GetType() == objectType);
            if (alreadyExists != null)
            {
                Console.WriteLine("FindSingleton(type): found {0}=>{1}", objectType.FullName, alreadyExists.ReadableId);
                return alreadyExists;
            }

            Console.WriteLine("FindSingleton(type): {0} not found, creating new", objectType.FullName);
            return CreateNewStdObject(objectType);
        }

        public static T FindSingleton<T>() where T:StdObject
        {
            var x = FindSingleton(typeof (T)) as T;
            return x; 
        }

        public static T CreateNewStdObject<T>() where T:StdObject, new()
        {
            var x = CreateNewStdObject(typeof (T)) as T;
            return x; 
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
            AllObjects.Add(ob);
            Console.WriteLine("CreateNewStdObject: {0} => created {1}", type.FullName, ob.ReadableId);
            return ob;
        }

        /// <summary>
        /// Do as much as I can to forget about an object. 
        /// </summary>
        /// <param name="ob"></param>
        public static void RemoveStdObjectFromGame(StdObject ob)
        {
            ob.Destroy();
            AllObjects.Remove(ob);
        }

    }
}