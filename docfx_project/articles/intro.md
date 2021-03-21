# Getting started

Creating an Imp.NET application is simple. The following code example is a complete, working chat server/client with Imp.NET. Multiple clients can connect to the server and send each other messages, which will appear in their console windows.

```csharp
public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Press S for server, or any other key for client.");
        if(Console.ReadKey().Key == ConsoleKey.S)
        {
            ChatServer server = new ChatServer(10);
            server.Start();
            Console.ReadKey();
        }
        else
        {
            ChatClient client = new ChatClient();
            client.Connect("127.0.0.1", 10);

            IChatServer server = client.Server;
            while(true)
            {
                server.SendMessage(Console.ReadLine());
            }
        }
    }
}

[Shared]
public partial class ChatClient : ImpClient<IChatServer>
{
    public async Task WriteMessage(string message)
    {
        Console.WriteLine(message);
    }
}

[Shared]
public partial class ChatServer : ImpServer<IChatClient>
{
    public ChatServer(int port) : base(port) { }

    public async Task SendMessage(string message)
    {
        foreach(IChatClient client in ConnectedClients)
        {
            client.WriteMessage(message);
        }
    }
}
```

Observe the fact that no explicit networking code is necessary; the client and server are able to interact with one another using local, natural method calls.

### How it works

Like most networking libraries, Imp.NET utilizes the concept of servers and clients to simplify communications. Multiple clients may connect to a server, and the server may interact with each client to perform a variety of tasks. In the `Main` method of the above example, the user can choose whether to run a server or a client. If they choose to run a server, a new `ChatServer` object is created, and the `Start` method is called to begin listening for clients. Note that the server does no additional processing in the `Main` method - the code for sending messages to clients will be remotely invoked *by* clients. If the user chooses to run a client, a new `ChatClient` object is created, and it connects to the server. Then, it retrieves an `IChatServer` object - an interface representing the server object on the *server-side*. Finally, the client repeatedly reads what the user types, and calls the `SendMessage` method on the server object. Since the server object is held server-side, the method will be called *on the server*, not the client. The server will then proceed to broadcast the message to every client, calling the *client-side* method `WriteMessage`. Each client object will receive the message, and write it to their console.

### Shared classes

`ChatClient` and `ChatServer` are examples of shared classes, classes that may be passed across the network by reference. When a shared class is passed as a method argument or property value, instead of its values being copied across the network, a reference to it is created on the remote host. Calling a method on this reference results in the method executing on the *local* object. In the above example, the `server` variable in the `Main` method and the `client` variable in the `SendMessage` method contain remote shared objects. When the program calls their methods, the methods are remotely executed on the original copies of the server and client objects. See [shared classes](sclasses.md) for more details.