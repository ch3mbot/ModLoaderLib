namespace ModLoaderLib
{
    public static class ModUtils
    {
        public static bool ModMinVersionMet(string version, string requiredVersion)
        {
            Version ver = new Version(version);
            Version requiredVer = new Version(requiredVersion);
            return ver.CompareTo(requiredVer) >= 0;
        }
    }
}
