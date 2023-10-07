using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoaderLib
{


    public enum ModProblemState
    {
        fine = 0,
        warning = 1,
        error = 2,
        fatal = 3,
    }

    public class ActiveModInfo
    {
        public ModInfo info;
        public string path;
        public ModProblemState problemState;
        public List<string> problems;

        public ActiveModInfo(ModInfo info, string path)
        {
            this.info = info;
            this.path = path;
            this.problemState = 0;
            this.problems = new List<string>();
        }

        public string displayName => info.displayName;
        public string uniqueID => info.uniqueID;
        public string version => info.version;
        public List<ModRelation> relations => info.relations;



    }
}
