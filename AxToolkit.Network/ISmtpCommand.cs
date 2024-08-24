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
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace AxToolkit.Network;

public interface ISmtpCommand
{
    string Command { get; }
    Task<bool> Exec(SmtpSession smtp, string argsText);
}

public class SmtpHELOCommand : ISmtpCommand
{
    /* Rfc 2821 4.1.1.1
    These commands, and a "250 OK" reply to one of them, confirm that
    both the SMTP client and the SMTP server are in the initial state,
    that is, there is no transaction in progress and all state tables and
    buffers are cleared.

    Syntax:
         "HELO" SP Domain CRLF
    */

    public string Command => "HELO";
    public async Task<bool> Exec(SmtpSession smtp, string argsText)
    {
        smtp.Reset();
        await smtp.WriteLine($"250 {smtp.Server.Hostname} at your service, [{smtp.Remote}]");
        return true;
    }

}

public class SmtpEHLOCommand : ISmtpCommand
{
    /* Rfc 2821 4.1.1.1
    These commands, and a "250 OK" reply to one of them, confirm that
    both the SMTP client and the SMTP server are in the initial state,
    that is, there is no transaction in progress and all state tables and
    buffers are cleared.
    */

    public string Command => "EHLO";
    public async Task<bool> Exec(SmtpSession smtp, string argsText)
    {
        smtp.Reset();
        await smtp.WriteLine($"250-{smtp.Server.Hostname} at your service, [{smtp.Remote}]");
        await smtp.WriteLine($"250-PIPELINING");
        await smtp.WriteLine($"250-SIZE {smtp.Server.MaxMessageSize}");
        if (smtp.Certificate != null)
            await smtp.WriteLine($"250-STARTTLS");
        // await smtp.WriteLine($"250-DSN");
        // await smtp.WriteLine($"250-HELP");
        await smtp.WriteLine($"250-8BITMIME");
        await smtp.WriteLine($"250-BINARYMIME");
        await smtp.WriteLine($"250-CHUNKING");
        await smtp.WriteLine($"250-AUTH LOGIN CRAM-MD5");
        await smtp.WriteLine($"250 OK");
        return true;
    }
}

public class SmtpMAILCommand : ISmtpCommand
{
    /* RFC 2821 3.3
    NOTE:
        This command tells the SMTP-receiver that a new mail transaction is
        starting and to reset all its state tables and buffers, including any
        recipients or mail data.  The <reverse-path> portion of the first or
        only argument contains the source mailbox (between "<" and ">"
        brackets), which can be used to report errors (see section 4.2 for a
        discussion of error reporting).  If accepted, the SMTP server returns
         a 250 OK reply.

        MAIL FROM:<reverse-path> [SP <mail-parameters> ] <CRLF>
        reverse-path = "<" [ A-d-l ":" ] Mailbox ">"
        Mailbox = Local-part "@" Domain

        body-value ::= "7BIT" / "8BITMIME" / "BINARYMIME"

        Examples:
            C: MAIL FROM:<ned@thor.innosoft.com>
            C: MAIL FROM:<ned@thor.innosoft.com> SIZE=500000 BODY=8BITMIME
    */
    public string Command => "MAIL";
    public async Task<bool> Exec(SmtpSession smtp, string argsText)
    {
        if (!smtp.Ready)
        {
            await smtp.WriteLine("503 Bad sequence of commands");
            return false;
        }
        else if (smtp.Sender != null)
        {
            await smtp.WriteLine("503 Sender already specified");
            return false;
        }

        var fromOpt = string.Empty;
        var sizeOpt = 0L;
        var transfertEncoding = TransferEncoding.SevenBit;
        var param = SmtpSession.ParseParams(argsText,
            @"(?<param>FROM)[\s]{0,}:\s{0,}<?\s{0,}(?<value>[\w\@\.\-\*\+\=\#\/]*)\s{0,}>?(\s|$)",
            @"(?<param>SIZE)[\s]{0,}=\s{0,}(?<value>[\w]*)(\s|$)",
            @"(?<param>BODY)[\s]{0,}=\s{0,}(?<value>[\w]*)(\s|$)").ToArray();
        foreach (var parameter in param)
        {
            switch (parameter.Key)
            {
                case "FROM":
                    fromOpt = parameter.Value;
                    break;

                case "SIZE":
                    if (!long.TryParse(parameter.Value, out sizeOpt))
                    {
                        await smtp.WriteLine("501 SIZE parameter value is invalid. Syntax:{MAIL FROM:<address> [SIZE=msgSize] [BODY=8BITMIME]}");
                        return false;
                    }
                    break;

                case "BODY":
                    if (string.IsNullOrEmpty(parameter.Value))
                    {
                        await smtp.WriteLine("501 BODY parameter value isn't specified. Syntax:{MAIL FROM:<address> [SIZE=msgSize] [BODY=8BITMIME]}");
                        return false;
                    }
                    switch (parameter.Value.ToUpper())
                    {
                        case "7BIT":
                            transfertEncoding = TransferEncoding.SevenBit;
                            break;
                        case "8BITMIME":
                            transfertEncoding = TransferEncoding.EightBit;
                            break;
                        case "BINARYMIME":
                            transfertEncoding = TransferEncoding.Base64;
                            break;
                        default:
                            await smtp.WriteLine("501 BODY parameter value is invalid. Syntax:{MAIL FROM:<address> [BODY=(7BIT/8BITMIME)]}");
                            return false;
                    }
                    break;

                default:
                    await smtp.WriteLine("501 Error in parameters. Syntax:{MAIL FROM:<address> [SIZE=msgSize] [BODY=8BITMIME]}");
                    return false;
            }
        }

        if (string.IsNullOrEmpty(fromOpt))
        {
            await smtp.WriteLine("501 Required param FROM: is missing. Syntax:{MAIL FROM:<address> [SIZE=msgSize] [BODY=8BITMIME]}");
            return false;
        }
        else if (smtp.Server.MaxMessageSize < sizeOpt)
        {
            await smtp.WriteLine("552 Message exceeds allowed size");
            return false;
        }

        var sender = new MailAddress(fromOpt);
        if (!await smtp.Server.ValidateSender(sender))
        {
            await smtp.WriteLine($"550 Sender {sender} is refused to send mail here");
            return false;
        }

        smtp.Reset();
        smtp.Sender = sender;
        smtp.TransferEncoding = transfertEncoding;
        await smtp.WriteLine($"250 OK {sender}");
        return true;
    }

}

public class SmtpRCPTCommand : ISmtpCommand
{
    /* RFC 2821 4.1.1.3 RCPT
    NOTE:
        This command is used to identify an individual recipient of the mail
        data; multiple recipients are specified by multiple use of this
        command.  The argument field contains a forward-path and may contain
        optional parameters.

        Relay hosts SHOULD strip or ignore source routes, and
        names MUST NOT be copied into the reverse-path.  

        Example:
            RCPT TO:<@hosta.int,@jkl.org:userc@d.bar.org>

            will normally be sent directly on to host d.bar.org with envelope
            commands

            RCPT TO:<userc@d.bar.org>
            RCPT TO:<userc@d.bar.org> SIZE=40000

        RCPT TO:<forward-path> [ SP <rcpt-parameters> ] <CRLF>			
    */

    public string Command => "RCPT";

    public async Task<bool> Exec(SmtpSession smtp, string argsText)
    {
        if (!smtp.Ready)
        {
            await smtp.WriteLine("503 Bad sequence of commands");
            return false;
        }

        var rcptOpt = string.Empty;
        var sizeOpt = 0L;
        var param = SmtpSession.ParseParams(argsText,
            @"(?<param>TO)[\s]{0,}:\s{0,}<?\s{0,}(?<value>[\w\@\.\-\*\+\=\#\/]*)\s{0,}>?(\s|$)",
            @"(?<param>SIZE)[\s]{0,}=\s{0,}(?<value>[\w]*)(\s|$)");
        foreach (var parameter in param)
        {
            switch (parameter.Key)
            {
                case "TO":
                    rcptOpt = parameter.Value;
                    if (string.IsNullOrEmpty(rcptOpt))
                    {
                        await smtp.WriteLine("501 Required param FROM: is missing. Syntax:{RCPT TO:<address> [SIZE=msgSize]}");
                        return false;
                    }
                    break;

                case "SIZE":
                    if (!long.TryParse(parameter.Value, out sizeOpt))
                    {
                        await smtp.WriteLine("501 SIZE parameter value is invalid. Syntax:{RCPT TO:<address> [SIZE=msgSize] }");
                        return false;
                    }
                    break;

                default:
                    await smtp.WriteLine("501 Error in parameters. Syntax:{RCPT TO:<address> [SIZE=msgSize]}");
                    return false;
            }
        }


        var recipient = new MailAddress(rcptOpt);
        if (smtp.Server.MaxMessageSize < sizeOpt)
        {
            await smtp.WriteLine("552 Message exceeds allowed size");
            return false;
        }

        var rcptInfo = await smtp.Server.ValidateRecipient(recipient, sizeOpt);
        if (!rcptInfo.Item1)
        {
            if (!string.IsNullOrWhiteSpace(rcptInfo.Item2)) 
                await smtp.WriteLine(rcptInfo.Item2); // 552 Mailbox size limit exceeded
            else
                await smtp.WriteLine($"550 {recipient} No such user here");
            return true;
        }
        else if (smtp.Recipients.Count > smtp.Server.MaxRecipients)
        {
            await smtp.WriteLine("452 Too many recipients");
            return false;
        }

        recipient = new MailAddress(rcptOpt, rcptInfo.Item2);
        smtp.Recipients.Add(recipient);
        await smtp.WriteLine($"250 OK {recipient}");
        return true;
    }
}

public class SmtpDATACommand : ISmtpCommand
{
    public string Command => "DATA";

    public async Task<bool> Exec(SmtpSession smtp, string argsText)
    {
        if (argsText.Length > 0)
        {
            await smtp.WriteLine("500 Syntax error. Syntax:{DATA}");
            return true;
        }
        else if (!smtp.Ready || smtp.Recipients.Count == 0)
        {
            await smtp.WriteLine("554 no valid recipients given");
            return true;
        }

        await smtp.WriteLine("354 Start mail input; end with <CRLF>.<CRLF>");

        var data = new StringBuilder();
        var inErr = false;
        for (; ; )
        {
            var line = await smtp.ReadLine();
            if (line == ".")
                break;
            if (line.StartsWith(".."))
                line = line.Substring(1);

            if (!inErr && data.Length > smtp.Server.MaxMessageSize)
            {
                inErr = true;
                await smtp.WriteLine("552 Requested mail action aborted: exceeded storage allocation");
            }
            else if (!inErr)
                data.AppendLine(line);
        }

        if (!inErr)
        {
            var mem = new MemoryStream();
            mem.Write(Encoding.UTF8.GetBytes(data.ToString()));
            if (!await smtp.Server.Message(mem, smtp.Sender!, smtp.Recipients))
            {
                await smtp.WriteLine("500 Internal error.");
                smtp.Reset();
                return true;
            }
            await smtp.WriteLine("250 OK");
        }
        smtp.Reset();
        return true;
    }
}

public class SmtpBDATCommand : ISmtpCommand
{
    public string Command => "BDAT";

    public async Task<bool> Exec(SmtpSession smtp, string argsText)
    {
        var args = argsText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        int chunkSize; 
        if (args.Length != 1 && args.Length != 2)
        {
            await smtp.WriteLine("500 Syntax error. Syntax:{BDAT <size> [LAST]}");
            return true;
        }
        else if (!smtp.Ready || smtp.Recipients.Count == 0)
        {
            await smtp.WriteLine("554 no valid recipients given");
            return true;
        }
        else if (!int.TryParse(args[0], out chunkSize))
        {
            await smtp.WriteLine("500 Syntax error. chunk size must be an integer value.");
            return true;
        }
        else if (smtp.ChunkedLength + chunkSize > smtp.Server.MaxMessageSize)
        {
            smtp.Chunk();
            await smtp.WriteLine("552 Requested mail action aborted: exceeded storage allocation");
            return true;
        }

        var bytes = await smtp.ReadData(chunkSize);
        if (bytes == null || bytes.Length != chunkSize)
        {
            smtp.Chunk();
            await smtp.WriteLine("500 UnKnownError");
            return true;
        }

        smtp.ChunkPush(bytes);
        if (args.Length != 2)
            await smtp.WriteLine($"250 {chunkSize} bytes received.");
        else
        {
            // LAST
            if (!await smtp.Server.Message(smtp.Chunk(), smtp.Sender!, smtp.Recipients))
            {
                await smtp.WriteLine("500 Internal error.");
                smtp.Reset();
                return true;
            }
        }
        return true;
    }
}
public class SmtpRSETCommand : ISmtpCommand
{
    /* RFC 2821 4.1.1
    NOTE:
        Several commands (RSET, DATA, QUIT) are specified as not permitting
        parameters.  In the absence of specific extensions offered by the
        server and accepted by the client, clients MUST NOT send such
        parameters and servers SHOULD reject commands containing them as
        having invalid syntax.
    */
    public string Command => "RSET";

    public async Task<bool> Exec(SmtpSession smtp, string argsText)
    {
        if (argsText.Length > 0)
        {
            await smtp.WriteLine("500 Syntax error. Syntax:{RSET}");
            return true;
        }

        smtp.Reset();
        await smtp.WriteLine("250 OK");
        return true;
    }
}
public class SmtpNOOPCommand : ISmtpCommand
{
    public string Command => "NOOP";

    public async Task<bool> Exec(SmtpSession smtp, string argsText)
    {
        await smtp.WriteLine("250 OK");
        return true;
    }
}
public class SmtpQUITCommand : ISmtpCommand
{
    public string Command => "QUIT";

    public async Task<bool> Exec(SmtpSession smtp, string argsText)
    {
        if (argsText.Length > 0)
            await smtp.WriteLine("500 Syntax error. Syntax:{QUIT}");
        else
            await smtp.WriteLine("221 Service closing transmission channel");
        return false;
    }
}
public class SmtpVRFYCommand : ISmtpCommand
{
    public string Command => "VRFY";

    public async Task<bool> Exec(SmtpSession smtp, string argsText)
    {
        await smtp.WriteLine("502 Command VRFY not implemented");
        return true;
    }
}
public class SmtpEXPNCommand : ISmtpCommand
{
    public string Command => "EXPN";

    public async Task<bool> Exec(SmtpSession smtp, string argsText)
    {
        await smtp.WriteLine("502 Command EXPN not implemented");
        return true;
    }
}
public class SmtpHELPCommand : ISmtpCommand
{
    public string Command => "HELP";

    public async Task<bool> Exec(SmtpSession smtp, string argsText)
    {
        await smtp.WriteLine("502 Command HELP not implemented");
        return true;
    }
}

public class SmtpSTARTTLSCommand : ISmtpCommand
{
    public string Command => "STARTTLS";

    public async Task<bool> Exec(SmtpSession smtp, string argsText)
    {
        if (smtp.Certificate == null)
        {
            await smtp.WriteLine("502 Command STARTTLS not implemented");
            return false;
        }
        await smtp.WriteLine("250 OK");
        smtp.Update();
        return true;
    }
}
