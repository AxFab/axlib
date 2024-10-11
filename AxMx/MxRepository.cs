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
using MongoDB.Driver;

namespace AxMx
{
    public class MxRepository : IMxRepository
    {
        private IMongoCollection<MxMessage> _messages;
        private IMongoCollection<MxThread> _threads;
        private IMongoCollection<MxUser> _users;
        private IMongoCollection<MxAddressBook> _books;
        private IMongoDatabase _database;

        public MxRepository()
        {
            Reset();
        }

        public void Reset()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            _database = mongoClient.GetDatabase("Mx");
            _messages = null;
            _threads = null;
            _users = null;
            _books = null;
        }


        protected IMongoCollection<MxMessage> Messages => _messages ??= _database.GetCollection<MxMessage>("Messages");
        protected IMongoCollection<MxThread> Threads => _threads ??= _database.GetCollection<MxThread>("Threads");
        protected IMongoCollection<MxUser> Users => _users ??= _database.GetCollection<MxUser>("Users");
        protected IMongoCollection<MxAddressBook> Books => _books ??= _database.GetCollection<MxAddressBook>("Books");

        // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

        private bool IsValidLang(string lang)
        {
            if (lang.Length != 2 && lang.Length != 5)
                return false;

            if (!char.IsAsciiLetterLower(lang[0]) || !char.IsAsciiLetterLower(lang[1]))
                return false;

            if (lang.Length == 2)
                return true;

            if (lang[2] != '-')
                return false;

            if (!char.IsAsciiLetterUpper(lang[3]) || !char.IsAsciiLetterUpper(lang[4]))
                return false;

            return true;
        }

        private void LogIssue(string error)
        {
            Console.Error.WriteLine(error);
        }

        // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
        public async Task Insert(MxUser value) => await Users.InsertOneAsync(value);
        public async Task Update(MxUser value) => await Users.ReplaceOneAsync(x => x.Id == value.Id, value);
        public async Task NewUser(string display, string user, string domain, string[] langs)
        {
            user = user.Trim();
            domain = domain.Trim();
            langs = langs.Where(x => x != null && IsValidLang(x)).ToArray();
            var isUserExist = (await Users.FindAsync(x => x.Domain == domain && x.Username == user, new FindOptions<MxUser, MxUser>
            {
                Limit = 1,
            })).Any();
            if (isUserExist)
                return;
            await Users.InsertOneAsync(new MxUser
            {
                Created = DateTime.UtcNow,
                Display = display,
                Domain = domain,
                Username = user,
                Languages = langs.ToList(),
                LastConnection = DateTime.UtcNow,
            });
        }

        public async Task<MxUser> SearchUser(string user, string domain)
        {
            user = user.Trim();
            domain = domain.Trim();
            var list = await Users.Find(x => x.Domain == domain && x.Username == user).Limit(2).ToListAsync();
            if (list.Count == 0)
                return null;
            if (list.Count > 1)
                LogIssue($"Duplicate user: {user}@{domain}");
            return list.First();
        }

        // -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
        public async Task Insert(MxMessage value) => await Messages.InsertOneAsync(value);
        public async Task Update(MxMessage value) => await Messages.ReplaceOneAsync(x => x.Id == value.Id, value);


        public async Task Insert(MxThread value) => await Threads.InsertOneAsync(value);
        public async Task Update(MxThread value) => await Threads.ReplaceOneAsync(x => x.Id == value.Id, value);

        public async Task<MxThread> SearchThread(Guid? uid, string subject)
        {
            MxThread thread = null;
            if (uid.HasValue)
                thread = await Threads.Find(x => x.ThreadId == uid.Value).FirstOrDefaultAsync();
            thread ??= await Threads.Find(x => x.Subject == subject).SortByDescending(x => x.LastUpdate).FirstOrDefaultAsync();
            return thread;
        }
    }

}
