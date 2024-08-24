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
using AxToolkit.Network;
using MongoDB.Driver;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace AxMx
{
    public class MxOptions
    {
        public class MxOptUser
        {
            public string Display { get; set; }
            public string User { get; set; }
            public string Domain { get; set; }
            public string[] Langs { get; set; }
        }
        public string CertificatePath { get; set; }
        public string CertificatePassword { get; set; }
        public List<MxOptUser> Users { get; set; } = new List<MxOptUser>();
    }


    public class MxProgram : SmtpServer
    {
        public IMxRepository Repository { get; }
        public MxOptions Options { get; }
        public MxProgram()
        {
            Repository = new MxRepository();
            if (File.Exists("mx.json"))
                Options = JsonSerializer.Deserialize<MxOptions>(File.ReadAllText("mx.json")) ?? new MxOptions();
            else 
                Options = new MxOptions();
        }

        static void Main(string[] args)
        {
            var mx = new MxProgram();

            foreach (var usr in mx.Options.Users)
                mx.Repository.NewUser(usr.Display, usr.User, usr.Domain, usr.Langs);

            mx.Listen(TcpPorts.SMTP);
        }

        public override async Task<(bool, string)> ValidateRecipient(MailAddress recipient, long messageSize)
        {
            try
            {
                var user = await Repository.SearchUser(recipient.User, recipient.Host);
                return (user != null, user?.Display ?? string.Empty);
            } catch (Exception ex)
            {
                return (false, "");
            }
        }

        public override async Task<bool> ValidateSender(MailAddress sender)
        {
            return true;
        }

        private static List<MxAddress> ReadAddresses(Dictionary<string, string> headers, string key)
        {
            if (!headers.TryGetValue(key, out var mails))
                return new List<MxAddress>();
            var list = mails.Split(',')
                .Select(x => new MailAddress(x.Trim()))
                .Select(x => new MxAddress
                {
                    Display = x.DisplayName,
                    User = x.User,
                    Domain = x.Host,
                })
                .ToList();
            headers.Remove(key);
            return list;
        }

        public override async Task<bool> Message(Stream bodyStream, MailAddress sender, List<MailAddress> recipients)
        {
            // Parse parameters
            var headers = new Dictionary<string, string>();
            var headKey = new List<string>();
            for (; ; )
            {
                var line = NetTools.ReadLine(bodyStream);
                if (string.IsNullOrEmpty(line))
                    break;

                var idx = line.IndexOf(':');
                if (idx <= 0)
                    return false;
                var key = line.Substring(0, idx).Trim();
                var value = line.Substring(idx + 1).Trim();
                headKey.Add(key);
                headers.Add(key, value);
            }


            // Create message
            var msg = new MxMessage
            {
                MessageId = Guid.NewGuid(),
                Received = DateTime.UtcNow,
                Sender = ReadAddresses(headers, "Sender").SingleOrDefault(),
                From = ReadAddresses(headers, "From").SingleOrDefault(),
                To = ReadAddresses(headers, "To"),
                Cc = ReadAddresses(headers, "Cc"),
                ReplyTo = ReadAddresses(headers, "Reply-To"),
                SendUser = MxAddress.Map(sender),
                Recipients = recipients.Select(MxAddress.Map).ToList(),
            };
            if (headers.TryGetValue("Date", out var sendDate)) {
                var date = DateTimeOffset.Parse(sendDate);
                msg.Sended = date.UtcDateTime;
                headers.Remove("Date");
            }
            if (headers.TryGetValue("Subject", out var subject))
            {
                msg.Subject = subject;
                headers.Remove("Subject");
            }

            if (headers.TryGetValue("Content-Type", out var contentType))
            {
                msg.ContentType = contentType;
                headers.Remove("Content-Type");
            }

            msg.Content = NetTools.ReadData(bodyStream, (int)(bodyStream.Length - bodyStream.Position));
            if (headers.TryGetValue("Content-Transfer-Encoding", out var contentEncoding)) {
                if (contentEncoding == "base64")
                {
                    msg.Content = Convert.FromBase64String(Encoding.ASCII.GetString(msg.Content));
                }
            }

            msg.HeaderKeys = headKey;
            msg.Parameters = new Dictionary<string, string>();
            foreach (var header in headers)
                msg.Parameters.Add(header.Key, header.Value);
            await Repository.Insert(msg);


            // TODO -- SpamAssassin 
            // TODO -- Automatic Rules 

            // Create thread
            var creation = false;
            var thread = await Repository.SearchThread(null, subject);
            if (thread == null) {
                creation = true;
                thread = new MxThread
                {
                    ThreadId = Guid.NewGuid(),
                    Subject = subject,
                };
            }

            thread.LastUpdate = msg.Received;
            thread.Messages.Add(msg.MessageId);
            thread.AddActor(msg.Sender);
            thread.AddActor(msg.From);
            thread.AddActors(msg.To);
            thread.AddActors(msg.Cc);
            thread.AddActors(msg.ReplyTo);
            thread.AddActors(msg.Recipients);

            if (creation)
                await Repository.Insert(thread);
            else
                await Repository.Update(thread);

            return true;
        }

        private readonly object _x509Lock = new object();
        private X509Certificate? _x509Certificate = null;

        public override X509Certificate SelectCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate? remoteCertificate, string[] acceptableIssuers)
        {
            lock (_x509Lock)
            {
                return _x509Certificate ??= new X509Certificate(Options.CertificatePath, Options.CertificatePassword);
            }
        }
    }
}