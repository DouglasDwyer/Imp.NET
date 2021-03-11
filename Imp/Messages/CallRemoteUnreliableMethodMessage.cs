namespace DouglasDwyer.Imp.Messages
{
    internal class CallRemoteUnreliableMethodMessage : ImpMessage
    {
        public ushort ObjectID;
        public object[] Parameters;
        public ushort MethodID;

        public CallRemoteUnreliableMethodMessage(ushort obj, object[] arguments, ushort methodID)
        {
            ObjectID = obj;
            Parameters = arguments;
            MethodID = methodID;
        }
    }
}