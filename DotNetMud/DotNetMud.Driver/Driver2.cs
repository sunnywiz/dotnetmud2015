using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetMud.Driver
{
    /// <summary>
    /// This is what most mud code would know as the driver - the O/S of the mud, as it were. 
    /// It tries to offload what it can from MudHub
    /// </summary>
    public class Driver2<T> where T:IGameSpecifics, new()
    {
        private static Driver2<T> _instance;

        private readonly Dictionary<string, IInteractive> _connectionToPlayer;
        private readonly Dictionary<IInteractive, string> _playerToConnection;
        private readonly Dictionary<string, Action<string>> _registeredNextInputRedirects;
        private readonly List<StdObject> _allObjects;
        private readonly IGameSpecifics _gameSpecifics;

        public Driver2()
        {
            _connectionToPlayer = new Dictionary<string, IInteractive>();
            _playerToConnection = new Dictionary<IInteractive, string>();
            _registeredNextInputRedirects = new Dictionary<string, Action<string>>();
            _allObjects = new List<StdObject>();
            _gameSpecifics = new T();
        }

        public static Driver2<T> Instance
        {
            get
            {
                if (_instance == null) _instance = new Driver2<T>();
                return _instance;
            }
        }

        #region INTERNAL things are called from MudHub  -- not accessible to master / rest of the game -- no longer true. 

        internal void RegisterInteractive(IInteractive player, string connectionId)
        {
            _connectionToPlayer[connectionId] = player;
            _playerToConnection[player] = connectionId;
        }

        public void ReceiveUserCommand(string connectionId, string cmd)
        {
            Console.WriteLine("ReceivedUserCommand: {0} sent {1}",connectionId, cmd);
            IInteractive player;
            if (_connectionToPlayer.TryGetValue(connectionId, out player))
            {
                Action<string> action;
                if (_registeredNextInputRedirects.TryGetValue(connectionId, out action) && action != null)
                {
                    _registeredNextInputRedirects.Remove(connectionId);
                    action(cmd);
                    return;
                }
                player.ReceiveInput(cmd);
            }
        }

        public void ReceiveDisconnection(string connectionId, bool stopCalled)
        {
            Console.WriteLine("ReceivedDisconnection: {0} stopCalled {1}", connectionId, stopCalled);
            IInteractive player;
            if (_connectionToPlayer.TryGetValue(connectionId, out player))
            {
                _gameSpecifics.PlayerGotDisconnected(player, stopCalled);
            }
        }

        public void ReceiveNewPlayer(string connectionId)
        {
            var interactive = _gameSpecifics.CreateNewPlayer();
            Console.WriteLine("incoming connection {0} assigned to {1}", connectionId, interactive);
            RegisterInteractive(interactive, connectionId);
            _gameSpecifics.WelcomeNewPlayer(interactive);
        }

        #endregion

        public Action<string, string> SendToClientCallBack { get; set; }

        public void SendTextToPlayerObject(IInteractive player, string message)
        {
            Console.WriteLine("SendTextToPlayerObject: to {0} send {1}",player,message);
            string connectionId;
            if (_playerToConnection.TryGetValue(player, out connectionId))
            {
                SendToClientCallBack?.Invoke(connectionId, message);
            }
        }

        public void RedirectNextUserInput(IInteractive player, Action<string> action)
        {
            Console.WriteLine("RedirectNextUserInput: for {0} ", player);
            string connectionId;
            if (_playerToConnection.TryGetValue(player, out connectionId))
            {
                _registeredNextInputRedirects[connectionId] = action;
            }
        }

        public StdObject FindSingleton(Type objectType)
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


        // TODO: At some point in the future, will need to bring back string locators which load from assemblies
        // that are not known at compile time.   Probably not till we get to our first wizarding realm though. 
        public StdObject CreateNewStdObject(Type type)
        {
            if (type == null || !type.IsSubclassOf(typeof (StdObject))) return null;
            var ob = type.Assembly.CreateInstance(type.FullName, false) as StdObject;

            if (ob == null)
            {
                Console.WriteLine("CreateNewStdObject: {0} => created type was not StdObject, return null", type.FullName);
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
        public void RemoveStdObjectFromGame(StdObject ob)
        {
            ob.Destroy(); 
            _allObjects.Remove(ob);
        }

        public IInteractive[] ListOfInteractives()
        {
            return _connectionToPlayer.Values.ToArray(); 
        }

        public object RequestPoll(string connectionId, string pollName, object clientState)
        {
            IInteractive player;
            if (_connectionToPlayer.TryGetValue(connectionId, out player))
            {
                return player.RequestPoll(pollName,clientState);
            }
            return null; 
        }
    }
}