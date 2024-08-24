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

public class MxUser : MxAddress
{
    [BsonId]
    public ObjectId Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastConnection { get; set; }
    public List<string> Languages { get; set; }
}
