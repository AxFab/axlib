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
﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AxMx.Models.Db;

[Serializable]
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
