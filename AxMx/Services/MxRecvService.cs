using AxMx.Models.Db;
using MongoDB.Driver;

namespace AxMx.Services
{
    public class MxRecvService
    {
        public IMxRepository Repository { get; set; }
        public MxThread FindThread (Guid? threadId, string subject)
        {
            lock (Repository.WriteLock)
            {

            }
        }

        public void SaveAddress(IEnumerable<MxAddress> addresses)
        {
            var grouped = addresses.GroupBy(x => x.Domain);
            foreach (var grp in grouped)
            {
                lock (Repository.WriteLock)
                {
                    var book = Repository.Books.SingleOrDefault(x => x.Domain == grp.Key);
                    if (book != null)
                    {
                        foreach (var usr in grp)
                        {
                            var any = book.Users.SingleOrDefault(x => x.Username == usr.Username);
                            if (any = null)
                                book.Users.Add(usr);
                            else if (!string.IsNullOrEmpty(usr.Display))
                                any.Display = usr.Display;
                        }
                        Repository.Update(book);
                    }
                    else 
                    {
                        book = new MxAddressBook
                        {
                            Domain = grp.Key,
                            Users = grp.ToList(),
                        };
                        Repository.Insert(book);
                    }
                }
            }
        }

        public void SaveMessage(MxMessage message, MxThread thread)
        {
            lock (Repository.WriteLock)
            {

            }
        }

    }
}
