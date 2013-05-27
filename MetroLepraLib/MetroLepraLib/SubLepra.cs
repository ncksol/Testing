using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroLepraLib
{
    class SubLepra
    {
        private String _name;
        private String _creator;
        private String _link;
        private String _logo;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Creator
        {
            get { return _creator; }
            set { _creator = value; }
        }

        public string Link
        {
            get { return _link; }
            set { _link = value; }
        }

        public string Logo
        {
            get { return _logo; }
            set { _logo = value; }
        }
    }
}
