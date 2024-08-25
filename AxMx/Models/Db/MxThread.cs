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
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AxMx.Models.Db
{
    [Serializable]
    public class MxThread
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public Guid ThreadId { get; set; }
        public string Subject { get; set; }
        public List<Guid> Messages { get; set; } = new List<Guid>();
        public List<MxAddress> Actors { get; set; } = new List<MxAddress>();
        public List<string> Labels { get; set; } = new List<string>();
        public DateTime LastUpdate { get; internal set; }

        public void AddActor(MxAddress actor)
        {
            if (!Actors.Any(x => x.Domain == actor.Domain && x.User == actor.User))
                Actors.Add(actor);
        }
        public void AddActors(IEnumerable<MxAddress> actors)
        {
            foreach (var actor in actors)
                AddActor(actor);
        }
    }
}
