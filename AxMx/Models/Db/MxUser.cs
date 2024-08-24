using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AxMx.Models.Db;

public class MxUser : MxAddress
{
    [BsonId]
    public ObjectId Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastConnection { get; set; }
    public List<string> Languages { get; set; }
}
