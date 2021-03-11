namespace DouglasDwyer.Imp.Messages
{
    internal class RemoteSharedObjectReleasedMessage : ImpMessage
    {
        public int Count;
        public ushort ObjectID;

        public RemoteSharedObjectReleasedMessage(int count, ushort id)
        {
            Count = count;
            ObjectID = id;
        }
    }
}