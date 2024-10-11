using AxMx.Models.Api;
using AxMx.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxMx.Services
{
    public class MxReadService
    {
        public MxTextService TextService { get; set; }
        public IMxRepository Repository { get; set; }

        public List<MxThreadInfo> Threads (Guid? lastOne, string label, int count)
        {
            IEnumerable<MxThread> set = Repository.Threads.OrderByDescending(x => x.OrderStamp);
            if (!string.IsNullOrEmpty(label))
                set = set.Where(x => x.Labels.Contains(label));
            if (lastOne != null)
            {
                var last = Repository.Threads.Single(x => x.ThreadId == lastOne.Value);
                set = set.SkipWhile(x => x.OrderStamp <= last.OrderStamp);
            }
            return set.Take(count).Select(x => MxThreadInfo.Map(x)).ToList(); 
        }

        public MxThreadDetails Thread(Guid uid)
        {
            var thread = Repository.Threads.First(x => x.ThreadId == uid);
            var message = Repository.Messages.Where(x => x.ThreadId == uid).ToList();
            return MxThreadDetails.Map(thread, message, TextService.AsHtml);
        }

        public List<MxAddressBook> Books()
        {
            return Repository.Books.ToList(); // RAW !
        }
    }
}
