using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxMx.Models.Db
{
    public class MxAddressBook
    {
        public string Domain { get; set; }
        public bool IsIntern { get; set; }
        public bool IsBlackListed { get; set; }
        public List<MxAddress> Users { get; set; } = new List<MxAddress>();
    }
}
