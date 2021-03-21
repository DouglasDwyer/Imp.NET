# Shared classes

Shared classes are classes that can be marshalled by reference across the network. When they are passed as arguments to a remote method call, returned as the value of a remote property, or returned as the return value of a remote method, a reference to them is created on the remote host. This reference is in the form of a shared interface - each remote object may be interacted with by calling its interface methods, just as one would call a normal method. Any non-shared object, when sent across the network, will be serialized and passed by value.

### Declaring a class as shared

There are two ways to declare a class as shared. If the class and interface are both already defined, one can employ the `ShareAs` attribute to denote their relationship. If the class to be shared does not have an interface, it can be marked as `Shared`, and the Imp source generator will generate an interface for it.

#### The ShareAs attribute

Any type that implements an interface may be shared across the network using that interface. To mark a class as shared, add the following attribute to the output assembly of the target project:
```csharp
[assembly: ShareAs(typeof(SharedClass),typeof(ISharedInterface))]
```

Now, when objects of `SharedClass` are sent to the remote host, the remote host will receive an `ISharedInterface` object. When the remote host calls a method on this remote object, the method will be invoked on the original `SharedClass` object.

If source generators are unavailable, then the `ShareAs` attribute must be utilized for all shared types. In this case, the user must define both the classes and the interfaces that should be used in sharing.

#### The Shared attribute

Any user-defined class may be shared across the network using a custom interface generated automatically at compile time. For the `Shared` attribute to work properly, the target version of C# must support source generators, and the Imp generator must be enabled. To mark a class as shared, mark it as `partial`, and add the following attribute to it:
```csharp
[Shared]
public partial class SharedClass
```
This will automatically cause the Imp generator to create an interface called `ISharedClass` in the same namespace. To change the name/namespace of the interface, use the following attribute overload:
```csharp
[Shared("SomeNamespace.ISharedInterface")]
public partial class SomeClass
```
This will automatically cause the Imp generator to create an interface called `ISharedInterface` in `SomeNamespace`.

### Shared class behavior

Shared classes are the way in which the server and client interact. Method calls on remote shared interfaces allow the client and server to selectively expose remote behaviors to one another, which can be called at any given time. The following is an example of a shared class:

```csharp
[Shared]
public partial class SomeClass {
	
	public int Foo { get; set; }
	
	[Local]
	public int Bar { get; set; }
	
	public void Test() {}
	
	public async Task AsyncTest() {}
	
	public async Task<int> GetBar() => Bar;
	
	[Unreliable]
	public void UDPTest() {}
}
```

When a client-side instance of this class is sent to the server, the server will receive an `ISomeClass` object. Using the interface object, the server can interact with each of `SomeClass`'s members. In this case, while all of the properties and methods will behave normally client-side, they each have a different behavior server-side.

The server will be able to access `Foo` and `Test`, but because they are normal properties/methods, calls to them will be synchronous and blocking. The server-side code that calls `Foo` or `Test` will stop, wait until the client-side operation has completed, and then continue. Though Imp.NET does support synchronous operations like this, they are time-consuming and inefficient. As such, it is highly recommended to always use asynchronous operations.

The server will not be able to access `Bar`, because it is marked as `Local`. Local members are not included in the automatically-generated interface definition.

The server will be able to call both `AsyncTest` and `GetBar`, which will execute asynchronously. When the server calls either one of these methods, it will receive a `Task` object indicating that the remote method call is underway client-side. Once the client-side call finishes, the `Task` object will be updated to include the results of the operation.

Finally, the server will be able to call `UDPTest`, which will also execute asynchronously - the server-side code will continue before the operation completes. This is because `UDPTest` is an unreliable method, meaning that Imp.NET will send the method invocation command over UDP, not TCP. UDP is slightly faster than TCP, with less overhead, but unreliable method calls are never guaranteed to execute remotely. As such, unreliable methods should be used for sending temporary data - like the position of a player in a game - that will quickly be overwritten with new data.

When a remote method call is received, it is executed locally using a `TaskScheduler`. This means that the thread and synchronization context in which remote method calls execute can be precisely controlled. The default scheduler is used automatically.

### Shared class lifetime

When a shared class is sent across the network, its reference is tracked by the sender. This means that, if the server sends an object to the client, that object will not be garbage collected until it is no longer used client-side or server-side. To accomplish this, Imp.NET employs reference counting - it tracks when an object is alive on a remote host, and waits until the remote interface is garbage collected to remove the original object.

Note that this means Imp.NET does not support cyclical references in shared objects. If two shared objects cyclically reference each other, and go out of scope on both hosts, they will not be garbage collected, creating a memory leak. This memory leak will only be resolved when one of the hosts disconnects, and its object references are invalidated. To prevent memory leakage, avoid creating cyclical references, or clear/break any cyclical references before discarding shared objects.

### Shared class invalidation

When a client disconnects from a server, all of the remote shared classes that the client and server received from one another are invalidated. Any calls to the remote interfaces' members will throw an exception, as the remote host is no longer available. Therefore, be certain to keep in mind that shared classes may become invalidated at any time during application runtime. Design with the expectation that shared classes may not last, and do not attempt to use them beyond their host's lifetime. This restriction does not apply to local members of remote proxy classes, which can be defined using a class marked with the `ProxyFor` attribute.

### In, out, and ref keywords

Because of Imp.NET's asynchronous nature, the `in`, `out`, and `ref` keywords are not supported as parameter modifiers. If a method containing one of these keywords is remotely invoked, an exception will be thrown.