[![Nuget](https://img.shields.io/nuget/v/DouglasDwyer.Imp)](https://www.nuget.org/packages/DouglasDwyer.Imp)
[![Downloads](https://img.shields.io/nuget/dt/DouglasDwyer.Imp)](https://www.nuget.org/packages/DouglasDwyer.Imp)

# Imp.NET
Imp.NET is a fast, high-level, object-oriented C# networking library that supports the invocation of remote methods through proxy interface objects. Imp.NET combines the power of the `System.Reflection.Emit` namespace with the flexibility of source generators to provide a seamless networking experience, without the hassle of network streams or pesky sockets. With Imp.NET, one writes networked code that looks identical to fully local code. Its fast and powerful features make it the perfect choice for any C# application.

Imp.NET may optionally be used without its source generator.

### Features

With Imp.NET, you can:

- Write networked code with remote method calls that looks exactly like normal/local code
- Send shared objects as method arguments across the network via interfaces, and remotely invoke methods/properties on them as though they are local objects
- Send non-shared objects as method arguments across the network by value using a serializer
- Pass any class that inherits from an interface, including library classes, across the network as a shared interface
- Call remote methods synchronously or asynchronously with the use of `Task`s and the `await` keyword
- Use UDP and the `Unreliable` attribute to send data across the network with less overhead

### Installation

Imp.NET can be obtained as a Nuget package. To import it into your project, either download DouglasDwyer.Imp from the Visual Studio package manager or run the command `Install-Package DouglasDwyer.Imp` using the package manager console.

### Getting started

Please see [the documentation](https://douglasdwyer.github.io/Imp.NET/) for a complete guide on setting up an Imp.NET application.

### Example

Imp.NET abstracts away the details of networking by automatically generating remote proxy classes that inherit from shared interfaces. In the following client-side example, a client connects to a server, then gets a remote representation of the server object, which is of the user-defined type `IChatServer`. Then, the client repeatedly sends messages read from the console to the server. Despite the fact that the code is executing client-side, the call to `SendMessage` executes on the server, as the `server` variable is a remote/server-owned object:
```csharp
ChatClient client = new ChatClient();
client.Connect("127.0.0.1", 10);

IChatServer server = client.Server;
while(true)
{
    server.SendMessage(Console.ReadLine());
}
```
Thus, Imp.NET provides an incredibly simple way for clients and servers to interact. The user can define custom "shared" properties and methods. These members may then be accessed remotely, as though they existed in the local runtime.
