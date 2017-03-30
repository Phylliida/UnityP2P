# UnityP2P
Peer to peer networking in Unity using WebRTC and a free Heroku server for signaling.

The general idea is that you use the free WebRTC plugin for unity then use a free Heroku server as your signaling server. There are a lot of small details like the server shutting down automatically (even if you don't want it to) after a few minutes of inactivity that just took some fiddling to figure out but I have made small tweaks that fix those issues.
In general the way it works is one person "hosts" a "server" at an id they chose (this can be pretty much any string). The quotes are because unlike a traditional server all this does is two things:

1. The WebRTC protocol is such that anyone else that connects with that id is initially connected to that server

2. Once they are connected it sends them a list of all the current peers, and each current peer gets notified by the server that that peer joined. 

After that, a peer can send messages to another peer without going through the server, which is the definition of a peer to peer network :) 

Here's how to do it:

1. Make an account on https://www.heroku.com/. It is free and doesn't even require you to enter payment info
2. Go into https://dashboard.heroku.com and click New, then Create New App.
3. Pick some random app name or let it choose one for you it doesn't really matter because this is just the websocket url that no one will see. Mine is nameless-scrubland-88927 but insert yours into the commands below instead
4. Install the [heroku cli](https://devcenter.heroku.com/articles/heroku-cli), nodejs, and npm
5. Clone this repo

`git clone https://github.com/Phylliida/UnityP2P.git`

then copy the server code (HerokuServer) into a directory without the .git file

6. In that directory, to login call

`heroku login`

7. Then create a new git repo there that will store your server code (Heroku works by having a git repo you manage your server code with)

`git init`

`heroku git:remote -a nameless-scrubland-88927`

(except use your server instead)

8. Now deploy your server

`npm install`

`git add .`

`git commit -am "Initial upload"`

`git push -f heroku master`

The -f (force) you shouldn't usually do but we need to override the initial configuration and this is the easiest way to do that.

You can tweak the config.json file if you want but that isn't really needed. You should also generate your own certificates in production but these work fine for testing small projects.

9. Open Unity, then select the UnityProject folder in this git repo to open it
10. Open the scene ExampleP2P (in the Assets folder) if it is not already open
11. In the ExampleP2PServer and ExampleP2PClient scripts attached to the server and client objects (respectively), change the Signaling Server variable to

`wss://nameless-scrubland-88927.herokuapp.com`

except use your sever instead. Note that you don't add the ports on the end.

12. Press play. This runs a server and a client, so look at the debug logs and you should see things happening :)

I haven't documented things yet but if you look at the ExampleP2PServer and ExampleP2PClient code it is simple enough that it should give you a jist of what is going on


# How to use it

Once you've got it set up, the code should be fairly easy to use. `ExampleP2PClient.cs` and `ExapleP2PServer.cs` have full working examples but here are the fundamentals:

There is a `P2PServer` class and a `P2PClient` class. You can think of this as a traditional server-client setup, except that once clients connect they can send things directly to each other without going through the server. The P2PServer uses the heroku websockets as the signaling server, it is just another layer on top that lets a peer join and then discover other peers.


## Server

To initialize a `P2PServer` you do:

`
string signalingServer = "wss://my-herokuserver-1234.herokuapp.com"; // Use your server as discussed above here
string roomName = "hi";
P2PServer server = new P2PServer(signalingServer, roomName);\
`

The room name is a unique identifier that lets multiple P2P servers run at the same time. If I choose the identifier of, say, "hi", then any client that connects that chooses that same id will connect to me. Then they will be able to send messages to anyone else that has also connected to me with that identifier.

The server needs to poll itself internally to process messages. To get it to do this, just call

`
server.UpdateServer();
`

Each frame (putting this in `Update()` works well, for example).


Now to make callbacks that will be called when events occur, you do soemthing like the following

`
server.OnConnection += Server_OnConnection;
server.OnDisconnection += Server_OnDisconnection;
`

Where we define those functions as:

`
void Server_OnConnection(ConnectionId connectionId)
{
  Debug.Log("Client: " + connectionId.ToString() + " connected");
  // You can use this connectionId to send messages to that connected peer, this will be described below
}

void Server_OnDisconnection(ConnectionId connectionId)
{
  Debug.Log("Client: " + connectionId.ToString() + " disconnected");
}
`

The server will keep track of a list of peers internally so you don't need to store them yourself.

Now to actually receive messages, we can do:

`
server.OnReceivedMessage += Server_OnReceivedMessage;
`

Where

`
void Server_OnReceivedMessage(NetworkEvent message)
{
  // The data sent by the peer
  byte[] resBytes = message.MessageData;
  // The position at which the data actually begins
  int resBytesOffset = message.MessageData.Offset;
  // The actual length of the data
  int resBytesLen = message.MessageData.ContentLength;
  
  // So resBytes can contain the data they sent alongside some dummy data
  // Their data starts at resBytes[resBytesOffset]
  // and ends at resBytes[resBytesOffset+resBytesLen]
  
  // If you just want the actual sent byte array you can do
  byte[] receivedBytes = new byte[resBytesLen];
  Buffer.BlockCopy(resBytes, resBytesOffset, receivedBytes, 0, resBytesLen);
  
  // But this can be a little inefficient since you have to copy each message over to a new array
  


  // Who the message is from
  ConnectionId peer = message.ConnectionId;
  
  // Now you can do whatever you want with those bytes
  
  
  // As one example, here is how to convert bytes to a string:
  string msg = Encoding.UTF8.GetString(message.MessageData.Buffer, message.MessageData.Offset, message.MessageData.ContentLength);
  Debug.Log("Server received message: " + msg + " from client " + message.ConnectionId.ToString());
}
`

There are two ways to send messages, reliable and unreliable.

Reliable messages (TCP) will be always received exactly as sent, in the order that they are sent (assuming that the connection doesn't go down). You should use these for most things.

Unreliable messages (UDP) may be received more than once, may not be received at all, or may be received in a different order then you sent them. The only guarantee you have is that if you receive a message there will not be any errors in it and you will receive the entire message, not just a piece of it. Why would you use this? For some things like streaming audio or video having some lossy data is fine as long as most of the stuff gets over. This is also much faster because the checking in TCP to make the messages reliable takes time.

You can do

`
bool isReliable = (message.Type == NetEventType.ReliableMessageReceived);
`

To learn if the message you received was sent in a reliable way or not.

To send a message, there are two options: send to a specific peer:

`
// Make some byte[] data; above

// You can pick this
bool isReliable=True;

server.SendMessage(connectionId, data, isReliable);

// Or, if your byte array contains some dummy data, you can do


// Offset where your data actually begins
int dataOffset = 0;

// Length of non-dummy data
int dataLen = 0;

server.SendMessage(connectionId, data, dataOffset, dataLen, isReliable);
`

To send a message to everyone, you can do

`
server.SendMessageToAll(data, isReliable);
// or
server.SendMessage(data, dataOffset, dataLen, isReliable);
`


Once you are done with the server, you can close it via

`
server.Dispose();
`

You should probably do something like this because cleaning it up is necessary:

`
void OnApplicationQuit()
{
  server.Dispose();
}

private void OnDestroy()
{
  server.Dispose();
}
`

Don't worry about calling it multiple times, it checks if it is already disposed and doesn't do anything if `Dispose()` has already been called.

## Client

To initialize a client, you do

`
P2PClient client = new P2PClient(signalingServer, roomName);
`
The client needs to poll itself internally to process messages. To get it to do this, just call
`
client.UpdateClient();
`

Each frame (putting this in `Update()` works well, for example).

All of the other callbacks and methods are identical to the server.




