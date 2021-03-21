# Remote shared classes

When a shared class is sent across the network, the remote host receives an interface representing the object. A class, however, is needed to implement the interface. By default, Imp.NET generates custom classes to implement each shared interface at runtime. All of these generated classes, or remote shared classes, inherit from the `RemoteSharedObject` class.

Sometimes, it is desirable to define custom fields, methods, and properties for remote objects. To do so, one can utilize the `ProxyFor` attribute and define a custom remote base class. The following example is the custom base class for `IImpClient`. It allows clients to access the remote server object without a remote call:
```csharp
[ProxyFor(typeof(IImpClient))]
public abstract class RemoteImpClient : RemoteSharedObject, IImpClient
{
	public IImpServer Server { get; private set; }

	public RemoteImpClient(ushort path, ImpClient host) : base(path, host) {
		Server = host.Server;
	}
}
```
All custom base classes should be marked with the `abstract` keyword in addition to the `ProxyFor` attribute. They must also inherit from `RemoteSharedObject` and the interface they are meant to represent.