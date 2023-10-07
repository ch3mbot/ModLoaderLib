using Microsoft.VisualBasic;
using Stride.Core;
using Stride.Core.Extensions;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace ModLoaderLib
{
    public static class ModLoader
    {
        public static string ModFolderPath;
        public static IServiceRegistry Services;
        public static Dictionary<string, ActiveModInfo> AllModlist;
        public static HashSet<string> EnabledMods;
        public static List<string> OrderedList;

        public static void Initialize(string modFolder, IServiceRegistry services)
        {
            Services = services;
            ModFolderPath = modFolder;
            AllModlist = new Dictionary<string, ActiveModInfo>();
            EnabledMods = new HashSet<string>();
            OrderedList = new List<string>();
            LoadAndValidateModlist();
        }

        public static void LoadAndValidateModlist()
        {
            List<string>? foundEnabledList = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(ModFolderPath + "modlist"));

            if(foundEnabledList == null)
            {
                foundEnabledList = new List<string>();
                SaveEnabledModlist(); //maybe not needed
            }

            string[] modDirectories = Directory.GetDirectories(ModFolderPath);
            foreach (string dir in modDirectories)
            {
                ModInfo currentInfo;
                try
                {
                    currentInfo = JsonSerializer.Deserialize<ModInfo>(File.ReadAllText(dir + "\\modInfo.json"));
                }
                catch (FileNotFoundException)
                {
                    //mod info not found in folder?
                    continue;
                }


                ActiveModInfo activeInfo = new ActiveModInfo(currentInfo, dir, false);
                string acid = activeInfo.uniqueID;
                AllModlist.Add(acid, activeInfo);
            }
            for(int i = 0; i < foundEnabledList.Count; i++)
            {
                string acid = foundEnabledList[i];
                if(AllModlist.ContainsKey(acid))
                {
                    EnabledMods.Add(acid);
                    OrderedList.Add(acid);
                }
            }
            RecalculateAllModStates();
        }

        public static void AddModToEnabledList(string modID)
        {
            if (!AllModlist.ContainsKey(modID)) return;
            if (EnabledMods.Contains(modID)) return;
            ActiveModInfo actInf = AllModlist[modID];
            string acid = actInf.uniqueID;
            EnabledMods.Add(acid);
            OrderedList.Add(acid);
            RecalculateAllModStates();
        }

        public static void RemoveModFromEnabledList(string modID)
        {
            if (!AllModlist.ContainsKey(modID)) return;
            if (!EnabledMods.Contains(modID)) return;
            ActiveModInfo actInf = AllModlist[modID];
            string acid = actInf.uniqueID;
            EnabledMods.Remove(acid);
            OrderedList.Remove(acid);
            RecalculateAllModStates();
        }

        //moves a mod to below this index.
        public static void MoveEnabledMod(string modID, int index, bool above)
        {
            if (!AllModlist.ContainsKey(modID)) return;
            if (!EnabledMods.Contains(modID)) return;
            ActiveModInfo actInf = AllModlist[modID];
            string acid = actInf.uniqueID;
            int currentIndex = OrderedList.IndexOf(modID);
            if (currentIndex == index) return;
            OrderedList.Insert(above ? index : index - 1, acid);
            OrderedList.RemoveAt(currentIndex);
            RecalculateAllModStates();
        }

        //should this be called all the time?
        public static void RecalculateAllModStates()
        {
            //computer inverse mod relationship mapping (why did I do this?)
            Dictionary<string, HashSet<string>> inverseRelationMap = new Dictionary<string, HashSet<string>>();
            foreach(string modID in AllModlist.Keys)
            {
                ActiveModInfo actInf = AllModlist[modID];
                foreach(ModRelation rel in actInf.relations)
                {
                    if (!inverseRelationMap.ContainsKey(rel.uniqueID))
                        inverseRelationMap[rel.uniqueID] = new HashSet<string>();
                    inverseRelationMap[rel.uniqueID].Add(modID);
                }
            }

            //set state of each active mod info
            foreach (string modID in AllModlist.Keys)
            {
                ComputeState(AllModlist[modID]);
            }
        }

        public static ModPreLoadState ComputeState(ActiveModInfo modInfo)
        {
            int maxError = 0;
            modInfo.problems = new List<string>();
            modInfo.problemState = 0;
            foreach (ModRelation rel in modInfo.relations)
            {
                switch (rel.relationType)
                {
                    case ModRelationType.incompatible:
                        if (EnabledMods.Contains(rel.uniqueID))
                        {
                            maxError = Math.Max(maxError, 3);
                            modInfo.problems.Add("");
                        }
                        break;
                    case ModRelationType.required:
                        if (!EnabledMods.Contains(rel.uniqueID))
                        {
                            maxError = Math.Max(maxError, 3);
                            modInfo.problems.Add("");
                        }
                        break;
                    case ModRelationType.optional:
                        if (EnabledMods.Contains(rel.uniqueID))
                        {

                        }
                        break;
                }
            }

            return ModPreLoadState.fine;
        }

        //saved modlist. assumes queue is sorted
        public static void SaveEnabledModlist()
        {
            Queue<string> outputModlist = new Queue<string>();
            foreach (KeyValuePair<string, ActiveModInfo> kvp in EnabledModlist)
            {
                outputModlist.Enqueue(kvp.Key);
            }
            File.WriteAllText(ModFolderPath + "modlist", JsonSerializer.Serialize(outputModlist));
        }

        public static bool ModIsEnabled(string modID)
        {
            return EnabledModlist.ContainsKey(modID);
        }
    }
}
