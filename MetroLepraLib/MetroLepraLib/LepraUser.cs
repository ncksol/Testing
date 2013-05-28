using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroLepraLib
{
    public class LepraUser
    {
        public String Username { get; set; }
        public String Userpic { get; set; }
        public String Number { get; set; }
        public String RegistrationDate { get; set; }
        public String FullName { get; set; }
        public String Location { get; set; }
        public String Karma { get; set; }
        public String UserStat { get; set; }
        public String VoteStat { get; set; }
        public String[] Contacts { get; set; }
        public String Description { get; set; }
    }
}
