using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxMx.Models.Api
{
    public class MxThreadDetails : MxThreadInfo
    {

        public List<MxMessageDetails> Messages = new List<MxMessageDetails>();
    }
}
