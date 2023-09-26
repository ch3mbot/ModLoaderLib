using Microsoft.VisualBasic;
using Stride.Core;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ModLoaderLib
{
    public static class ModLoader
    {
        public static string ModFolder;
        public static IServiceRegistry Services;
        public static Dictionary<string, ActiveModInfo> ActiveModlist;

        public static void Initialize(string modFolder, IServiceRegistry services)
        {
            Services = services;
            ModFolder = modFolder;
            ActiveModlist = new Dictionary<string, ActiveModInfo>();
            LoadAndValidateModlist();
        }

        public static void DisableMod(ActiveModInfo actinf)
        {
            foreach(var kvp in ActiveModlist)
            {
                foreach(ModRelation rel in kvp.Value.info.relations)
                {
                    if(rel.internalID == actinf.info.internalID && rel.relationType == ModRelationType.required)
                    {
                        kvp.Value.enabled = false;
                    }
                }
            }
            actinf.enabled = false;
        }
        
        public static bool AttemptEnableMod(ActiveModInfo actinf)
        {
            foreach(ModRelation rel in actinf.info.relations)
            {
                if(rel.relationType == ModRelationType.incompatible && ActiveModlist[rel.internalID].enabled)
                {
                    //incompatible mod enabled
                    return false;
                }

                if (rel.relationType == ModRelationType.required && !ActiveModlist[rel.internalID].enabled)
                {
                    if (!AttemptEnableMod(ActiveModlist[rel.internalID]))
                    {
                        //couldnt enable a dependency
                        return false;
                    }
                }
            }
            return true;
        }

        public void AttemptLoadMods()
        {
            HashSet<ActiveModInfo> toLoad = new HashSet<ActiveModInfo>();
            
            foreach(KeyValuePair<string, ActiveModInfo> kvp in ActiveModlist)
            {
                if (kvp.Value.enabled)
                {
                    toLoad.Add(kvp.Value);
                }
            }

            foreach(ActiveModInfo modinf in toLoad)
            {
                AttemptLoadMod(modinf);
            }
        }

        public static bool AttempLoadMod(ActiveModInfo modinf)
        {
            //see if matching version dependencies
        }

        public static bool IsModLoaded(string modid)
        {
            if (!ActiveModlist.TryGetValue(modid, out ActiveModInfo? modinfo))
                return false;
            return modinfo == null ? false : modinfo.loaded;
        }

        public static void LoadAndValidateModlist()
        {
            Dictionary<string, bool>? foundList = JsonSerializer.Deserialize<Dictionary<string, bool>>(File.ReadAllText(ModFolder + "modlist"));
            Dictionary<string, bool> uFoundList = foundList ?? new Dictionary<string, bool>();

            string[] modDirectories = Directory.GetDirectories(ModFolder);
            foreach (string dir in modDirectories)
            {
                ModInfo currentInfo;
                try
                {
                    currentInfo = JsonSerializer.Deserialize<ModInfo>(File.ReadAllText(dir + "\\modInfo.json"));
                }
                catch (FileNotFoundException)
                {
                    continue;
                }

                bool found = uFoundList.TryGetValue(currentInfo.displayName, out bool enabled);

                ActiveModInfo activeInf = new ActiveModInfo(currentInfo, dir, found && enabled, false);
                ActiveModlist.Add(activeInf.info.internalID, activeInf);
            }
        }

        public static void SaveEnabledModlist()
        {
            Dictionary<string, bool> outputModlist = new Dictionary<string, bool>();
            foreach(KeyValuePair<string, ActiveModInfo> kvp in ActiveModlist)
            {
                outputModlist.Add(kvp.Key, kvp.Value.enabled);
            }
            File.WriteAllText(ModFolder + "modlist", JsonSerializer.Serialize(outputModlist));
        }
    }
}
