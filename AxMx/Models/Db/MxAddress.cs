using System.Net.Mail;

namespace AxMx.Models.Db;

public class MxAddress
{
    public string Display { get; set; }
    public string User { get; set; }
    public string Domain { get; set; }
    public string Address => $"{User}@{Domain}";
    public string FullString => $"{Display} <{User}@{Domain}>";

    public static MxAddress Map(MailAddress src)
    {
        return new MxAddress
        {
            User = src.User,
            Domain = src.Host,
            Display = src.DisplayName,
        };
    }
}
