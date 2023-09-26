using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoaderLib
{
    public struct ActiveModInfo
    {
        public ModInfo info;
        public string path;
        public bool enabled;
        public bool loaded;

        public ActiveModInfo(ModInfo info, string path, bool enabled, bool loaded)
        {
            this.info = info;
            this.path = path;
            this.enabled = enabled;
            this.loaded = loaded;
        }
    }
}
