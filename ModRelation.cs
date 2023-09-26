namespace ModLoaderLib
{
    public enum ModRelationType
    {
        incompatible = -1,
        optional = 0,
        required = 1,
    }

    public struct ModRelation
    {

        public ModRelationType relationType;
        public string internalID;
        public string version;
    }
}
