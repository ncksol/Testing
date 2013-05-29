using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroLepraLib
{
    public class LepraComment
    {
        public String Id { get; set; }
        public String Text { get; set; }
        public Boolean IsNew { get; set; }
        public int Indent { get; set; }
        public String Rating { get; set; }
        public String When { get; set; }
        public int Vote { get; set; }
        public String User { get; set; }
    }
}
