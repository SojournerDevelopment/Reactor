# Implementing Server/Client

The server/client of the Reactor Framework provide you with two basic selections. The first one is the `ReactorServer` itself.

In addition, you can derive from `ReactorSecureServer` which provides basic security features.

#### Reactor Server

The ReactorServer is a stripped down model to build you own logic and create a powerfufl tcp server. To create a custom Server please perform the following steps:

* Create the Server Class - Inherit `ReactorServer`
* Create the Virtual Client Class - Inherit `ReactorVirtualClient`
 
The server class itself contains basic operations and information for the server. In addition, you have to create your own clients via the virtual method `AcceptVirtualClient()` which must return an instance of `ReactorVirtualClient`.

The procedure above will add the newly created client to the clients collection.

```csharp
    public class CustomServer : Core.ReactorServer
    {
        public Dictionary<string, CustomVirtualClient> ClientDictionary
        {
            get
            {
                Dictionary<string, CustomVirtualClient> clients = new Dictionary<string, CustomVirtualClient>();
                foreach (var e in base.Clients)
                {
                    clients.Add(e.Key, (CustomVirtualClient)e.Value);
                }
                return clients;
            }
        }
        
        public ReactorSecureServer() : base() { }

        /*
        #  This creates the CustomVirtualClient instance for the base
        #  logic to keep in the client collection.
        */
        protected override CustomVirtualClient AcceptVirtualClient()
        {
            return new CustomVirtualClient(this);
        }

        #region Overrides

        protected override void ClientConnected(ReactorVirtualClient client)
        {
            // Custom connected logic here
            CustomVirtualClient client = (CustomVirtualClient)client;
        }

        protected override void ClientCrashed(ReactorVirtualClient client)
        {
            // Custom crashed logic
            CustomVirtualClient client = (CustomVirtualClient)client;
        }

        protected override void ClientDisconnected(ReactorVirtualClient client)
        {
            // Custom disconnected logic
            CustomVirtualClient client = (CustomVirtualClient)client;
        }

        #endregion

    }
```

The next step is the VirtualClient implementation. In this example, the server will simply send strings of text through the network.

Packet handling logic is implemented in the following class.

```csharp
    public class CustomVirtualClient : ReactorVirtualClient
    {

        public CustomVirtualClient(Core.ReactorServer server) : base(server) {}

        #region Overrides

        protected override void ClientCrashed()
        {
            // Possible to reconnect the server automatically.
        }

        /*
        # This method is overriden to provide functionality of giving
        # your clients custom identification signs.
        */
        protected override string GenerateId()
        {
            // The base method generates a GUID
            return base.GenerateId();
        }

        /*
        # Handle incomming data here.
        # This example parses the received data as a string
        */
        protected override void Handle(byte[] data)
        {
            string received = Encoding.Unicode.GetString(data);
            Console.WriteLine("Received: "+received);
        }

        /*
        # This defines the packet, that is sent to the client after the
        # connection was accepted. Uses: Send packet with ID of client..
        */
        public override void SendRegistration()
        {
            string packet = "HELLO WORLD";
            byte[] byte_packet = Encoding.Unicode.GetBytes(packet);
            // Make sure to use this method to send bytes
            SendPacket(byte_packet);
            // LEAVE THIS METHOD EMPTY TO SEND NO REGISTRATION
        }

        /*
        # Here you can define the Packet that is send to the client,
        # before the server closes the connection.
        */
        public override void SendDisconnect()
        {
            base.SendDisconnect();
        }

        #endregion

    }
```

With this easy implementation, you get a fully functional TCP Server, that can handle you own clients and is powered by your own logic.

The next step will teach you how to create a custom client, that is directly working with your server.

#### Reactor Client

