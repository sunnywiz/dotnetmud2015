using System;
using System.Collections.Generic;
using System.Linq;
using DotNetMud.A.MudLib;

namespace DotNetMud.A.Server
{
    /// <summary>
    /// This is what most mud code would know as the driver - the O/S of the mud, as it were. 
    /// It tries to offload what it can from MudHub
    /// </summary>
    public class Driver<T> where T:IGameSpecifics, new()
    {
        private static Driver<T> _instance;

        private readonly Dictionary<string, IInteractive> _connectionToPlayer;
        private readonly Dictionary<IInteractive, string> _playerToConnection;
        private readonly Dictionary<string, Action<string>> _registeredNextInputRedirects;
        private readonly List<StdObject> _allObjects;
        private readonly IGameSpecifics _gameSpecifics;

        public Driver()
        {
            _connectionToPlayer = new Dictionary<string, IInteractive>();
            _playerToConnection = new Dictionary<IInteractive, string>();
            _registeredNextInputRedirects = new Dictionary<string, Action<string>>();
            _allObjects = new List<StdObject>();
            _gameSpecifics = new T();
        }

        public static Driver<T> Instance
        {
            get
            {
                if (_instance == null) _instance = new Driver<T>();
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

        /// <summary>
        /// Attempts to find the singleton specified.  If not already created, creates it. 
        /// </summary>
        /// <param name="uri"></param>
        public StdObject FindSingletonByUri(string uri)
        {
            var parsed = new Uri(uri);
            var combed = parsed.ToString(); 
            // TODO: probably need to optimize this lookup.
            var alreadyExists = _allObjects.FirstOrDefault(x => x.TypeUri == combed);
            if (alreadyExists != null)
            {
                Console.WriteLine("FindSingletonbyUri: found {0}=>{1} ({2}) ", uri,alreadyExists.ObjectId,alreadyExists.TypeUri);
                return alreadyExists;
            }

            Console.WriteLine("FindSingletonByUri: {0} not found, creating new",uri);
            return CreateNewStdObject(uri);
        }

        public StdObject CreateNewStdObject(string uri)
        {
            var parsed = new Uri(uri);
            if (parsed.Scheme == "builtin")
            {
                var typeName = parsed.Host.Trim();  // this typeName is lowercased. 

                var type = this.GetType().Assembly.GetType(typeName,false, true);
                if (type != null && type.IsSubclassOf(typeof(StdObject)))
                {
                    var ob = this.GetType().Assembly.CreateInstance(
                        parsed.Host,true) as StdObject;
                    if (ob == null)
                    {
                        Console.WriteLine("CreateNewStdObject: {0} => created type was not StdObject, return null",uri);
                        return null;
                    }
                    ob.TypeUri = parsed.ToString(); 
                    _allObjects.Add(ob);
                    Console.WriteLine("CreateNewStdObject: {0} => created {1} ({2})", uri, ob.ObjectId,ob.TypeUri);
                    return ob;
                }
                Console.WriteLine("CreateNewStdObject: {0} => type does not appear to be a StdObject", uri);
                return null;
            }
            Console.WriteLine("CreateNewStdObject: {0} => scheme {1} not known", parsed.Scheme);
            return null;
        }

        /// <summary>
        /// Do as much as I can to forget about an object. 
        /// </summary>
        /// <param name="ob"></param>
        public void RemoveStdObjectFromGame(StdObject ob)
        {
            ob.MoveTo(null);
            ob.IsDestroyed = true;
            _allObjects.Remove(ob);
        }

        /// <summary>
        /// Tell all the interactives (people who can receive messages) in a room something. 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="message"></param>
        public void TellRoom(StdObject room, string message)
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