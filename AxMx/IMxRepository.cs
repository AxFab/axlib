// This file is part of AxLib.
// 
// AxLib is free software: you can redistribute it and/or modify it under the
// terms of the GNU General Public License as published by the Free Software 
// Foundation, either version 3 of the License, or (at your option) any later 
// version.
// 
// AxLib is distributed in the hope that it will be useful, but WITHOUT ANY 
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more 
// details.
// 
// You should have received a copy of the GNU General Public License along 
// with AxLib. If not, see <https://www.gnu.org/licenses/>.
// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
using AxMx.Models.Db;

namespace AxMx
{
    public interface IBaseRepository<in T>
    {
        Task Insert(T value);
        Task Update(T value);
    }

    public interface IMxRepository : IBaseRepository<MxMessage>, IBaseRepository<MxThread>, IBaseRepository<MxUser>
    {
        public object WriteLock { get; }
        Task NewUser(string display, string user, string domain, string[] langs);
        Task<MxUser> SearchUser(string user, string domain);
        Task<MxThread> SearchThread(Guid? uid, string subject);
    }

}
