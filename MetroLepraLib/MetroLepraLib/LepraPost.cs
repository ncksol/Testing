using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroLepraLib
{
    public class LepraPost
    {
        public String Id { get; set; }
        public String Body { get; set; }
        public String Rating { get; set; }
        public String Url { get; set; }
        public String Image { get; set; }
        public String Text { get; set; }
        public String User { get; set; }
        public String Comments { get; set; }
        public String Wrote { get; set; }
        public String When { get; set; }
        public int Vote { get; set; }
        public string Type { get; set; }
    }
}
