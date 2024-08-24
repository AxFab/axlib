using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AxMx.Models.Db;

public class MxMessage
{
    [BsonId]
    public ObjectId Id { get; set; }
    public Guid MessageId { get; set; }
    public DateTime Sended { get; set; }
    public DateTime Received { get; set; }
    public string Subject { get; set; }
    public string ContentType { get; set; }
    public byte[] Content { get; set; }
    public MxAddress Sender { get; set; }
    public MxAddress From { get; set; }
    public List<MxAddress> To { get; set; }
    public List<MxAddress> Cc { get; set; }
    public List<MxAddress> ReplyTo { get; set; }
    public List<MxAddress> Recipients { get; set; }
    public Dictionary<string, string> Parameters { get; set; }
    public MxAddress SendUser { get; set; }
    public List<string> HeaderKeys { get; set; }
}
