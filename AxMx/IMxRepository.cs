using AxMx.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxMx
{
    public interface IBaseRepository<T>
    {
        Task Insert(T value);
        Task Update(T value);
    }

    public interface IMxRepository : IBaseRepository<MxMessage>, IBaseRepository<MxThread>, IBaseRepository<MxUser>
    {
        Task NewUser(string display, string user, string domain, string[] langs);
        Task<MxUser> SearchUser(string user, string domain);
        Task<MxThread> SearchThread(Guid? uid, string subject);
    }

}
