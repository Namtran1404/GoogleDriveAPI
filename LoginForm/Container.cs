using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveAPIExample
{
    public class Container
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string owner { get; set; }
        public string root { get; set; }
        public Container(string id, string name,string type,string owner,string root)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.owner = owner;
            this.root = root;
        }
    }
}
