# Imp.NET
Imp.NET is a fast, high-level, object-oriented C# networking library that supports the invocation of remote methods through proxy interface objects. Imp.NET combines the power of the `System.Reflection.Emit` namespace with the flexibility of source generators to provide a seamless networking experience, without the hassle of network streams or pesky sockets. With Imp.NET, one can write networked code that looks identical to fully local code. Its fast and powerful features make it the perfect choice for any C# application.

Imp.NET may optionally be used without its source generator.

### Overview

Imp.NET abstracts away the details of networking by automatically generating remote proxy classes that inherit from shared interfaces. Objects derived from shared interfaces can be sent from client to server and server to client, where they are represented by remote proxies. Methods can be called on remote proxies as though they are normal objects, and the method calls execute on the remote host. For example, the following code connects to a server and retrieves the remote server object, which is of the user-defined interface type `IChatServer`. The client is able to call the user-defined `SendMessage` method without any extra code, but the method will execute server-side:
```csharp
ChatClient client = new ChatClient();
client.Connect("127.0.0.1", 42069);

IChatServer server = client.Server;
while(true)
{
    server.SendMessage(Console.ReadLine());
}
```
Observe how interactions with a remote object are identical to interactions with a local object - this is Imp.NET's core feature. No interactions with serializers or streams are necessary.

### Concepts

The following concepts are important to understand when using Imp.NET:

- Server: Imp.NET employs a server-client architecture. Each application that would like to perform networking operations connects to the centralized server. Any application that will act as a server must define a class that inherits from `ImpServer`.
- Client: Each application that would like to interact with a server is a client. Any client application must define a class that inherits from `ImpClient`.
- Local shared class: When objects are sent across the network as method parameters, they are either copied by value or marshalled by reference. Classes that should be passed by reference are referred to as local shared classes. Shared classes are defined using the `Shared` or `ShareAs` attributes, and must inherit from a public interface. Local shared objects may be sent across the network as this interface.
- Shared interface: Objects that have been marshalled by reference each derive from a shared interface. This shared interface is how they can be interacted with across the network; when an object is received by a remote host, the remote host receives the interface. This means that all methods/properties expecting to deal with shared classes should utilize the shared interface as their argument/return types.
- Remote shared class: To utilize interfaces on the remote host, they must be implemented with a class. Imp.NET automatically generates a remote shared class for every shared interface using `Emit`. When a shared object is received on the remote host, it is always a remote shared type. All remote shared objects inherit from the `RemoteSharedObject` class.
