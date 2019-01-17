using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Installer
{


    public class JsonParser
    {
        public FirsrHirc Parse()
        { 
        string fileName = @"Config.JSON";
        string js = File.ReadAllText(fileName);
        FirsrHirc fJson = JsonConvert.DeserializeObject<FirsrHirc>(js);
        return fJson;
        }
    }

    public class FirsrHirc
    {
        public InstallList install { get; set; }
    }

    public class InstallList
    {
        public string name { get; set; }
        public string version { get; set; }
        public List<ImagesList> list { get; set; }

    }

    public class ImagesList
    {
        public string partition { get; set; }
        public string image { get; set; }
    }


   
}
