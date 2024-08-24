using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxMx.Models.Db
{
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
