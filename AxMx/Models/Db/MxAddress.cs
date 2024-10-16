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
﻿using System.Net.Mail;

namespace AxMx.Models.Db;

[Serializable]
public class MxAddress
{
    public string Display { get; set; }
    public string Username { get; set; }
    public string Domain { get; set; }
    public string Address => $"{Username}@{Domain}";
    public string FullString => string.IsNullOrEmpty(Display) ? $"{Username}@{Domain}" : $"{Display} <{Username}@{Domain}>";

    public static MxAddress Map(MailAddress src)
    {
        return new MxAddress
        {
            Username = src.User,
            Domain = src.Host,
            Display = src.DisplayName,
        };
    }
}
