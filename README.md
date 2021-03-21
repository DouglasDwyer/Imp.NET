[![Nuget](https://img.shields.io/nuget/v/DouglasDwyer.Imp)](https://www.nuget.org/packages/DouglasDwyer.Imp)
[![Downloads](https://img.shields.io/nuget/dt/DouglasDwyer.Imp)](https://www.nuget.org/packages/DouglasDwyer.Imp)

# Imp.NET
Imp.NET is a fast, high-level, object-oriented C# networking library that supports the invocation of remote methods through proxy interface objects. Imp.NET combines the power of the `System.Reflection.Emit` namespace with the flexibility of source generators to provide a seamless networking experience, without the hassle of network streams or pesky sockets. With Imp.NET, one writes networked code that looks identical to fully local code. Its fast and powerful features make it the perfect choice for any C# application.

Imp.NET may optionally be used without its source generator.

### Installation

Imp.NET can be obtained as a Nuget package. To import it into your project, either download DouglasDwyer.Imp from the Visual Studio package manager or run the command `Install-Package DouglasDwyer.Imp` using the package manager console.

### Features

- Write networked code with remote method calls that looks exactly like normal, local code
- Send shared objects across the network as interfaces and remotely invoke methods/properties on them as though they are local objects
- Any class that inherits from an interface, including library classes, may be marked as shared and passed by reference across the network
- Objects may be sent across the network as method/property arguments, and can be passed by value using a serializer in addition to passing by reference
- Remote method calls may occur synchronously or asynchronously with the use of `Task`s and the `await` keyword
- Use UDP and the `Unreliable` attribute to send data across the network with less overhead

### Overview

Imp.NET abstracts away the details of networking by automatically generating remote proxy classes that inherit from shared interfaces. First, the user defines shared local types - types whose methods and properties may be accessed across the network. Then, during runtime, an Imp.NET client application connects to an Imp.NET server application. These applications may send one another instances of shared local types. Shared local types each implement a shared interface, which defines the members that can be called remotely. When a shared object is received over the network, it is received as the shared interface type. The following client-side code example demonstrates remote method invocation. In the example, the remote server object (whose shared interface type is `IChatServer`) is retrieved, and the remote method `SendMessage` is invoked. Observe that the process is identical to calling normal methods:
```csharp
ChatClient client = new ChatClient();
client.Connect("127.0.0.1", 42069);

IChatServer server = client.Server;
while(true)
{
    server.SendMessage(Console.ReadLine());
}
```
Despite how simple this appears, the `SendMessage` method will be called server-side, even though client-side code is being executed. Any class that implements an interface may be marked as shared using the `Shared` and `ShareAs` attributes, including classes in other libraries.

### Getting started

Please see the documentation for a complete guide on setting up an Imp.NET application.
