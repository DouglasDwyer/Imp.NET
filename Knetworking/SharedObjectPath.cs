namespace DouglasDwyer.Knetworking
{
    public readonly struct SharedObjectPath
    {
        public readonly ushort OwnerID;
        public readonly ushort ObjectID;

        public SharedObjectPath(ushort owner, ushort obj)
        {
            OwnerID = owner;
            ObjectID = obj;
        }

        public override bool Equals(object obj)
        {
            return obj is SharedObjectPath path && this == path;
        }

        public override int GetHashCode()
        {
            return OwnerID ^ ObjectID;
        }

        public static bool operator ==(SharedObjectPath a, SharedObjectPath b)
        {
            return a.OwnerID == b.OwnerID && a.ObjectID == b.ObjectID;
        }

        public static bool operator !=(SharedObjectPath a, SharedObjectPath b)
        {
            return !(a == b);
        }
    }
}