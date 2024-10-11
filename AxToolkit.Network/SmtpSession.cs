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
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace AxToolkit.Network;

public class SmtpSession
{
    private Stream _input;
    private Stream _output;
    private MemoryStream? _chunkBuffer;

    static readonly object _lock = new object();
    static readonly Dictionary<string, ISmtpCommand> _commands = new Dictionary<string, ISmtpCommand>();
    static SmtpSession()
    {
        lock (_lock)
        {
            var smtpTypes = typeof(SmtpSession).Assembly.GetTypes()
                .Where(x => x.IsAssignableTo(typeof(ISmtpCommand)) && x != typeof(ISmtpCommand)).ToArray();
            var smtpCommands = smtpTypes
                .Select(x => (ISmtpCommand)Activator.CreateInstance(x)).ToArray();
            foreach (var cmd in smtpCommands)
            {
                Console.WriteLine($"Add command {cmd.Command}");
                _commands.Add(cmd.Command, cmd);
            }
        }
    }
    public SmtpSession(SmtpServer server, Stream input, Stream output)
    {
        Server = server;
        _input = input;
        _output = output;
    }

    public string Remote { get; set; } = "localhost";
    public SmtpServer Server { get; }
    public bool Ready { get; private set; }
    public MailAddress? Sender { get; set; }
    public List<MailAddress> Recipients { get; } = new List<MailAddress>();
    public TransferEncoding TransferEncoding { get; set; }
    public long ChunkedLength => _chunkBuffer?.Length ?? 0;

    private X509Certificate? _certificate;
    public X509Certificate? Certificate => _certificate ??= Server.SelectCertificate(null, "localhost", null, null, null);
    public void Reset()
    {
        Ready = true;
        Sender = null;
        Recipients.Clear();
        _chunkBuffer = null;
    }
    public void Update()
    {
        Reset();
        _input.Flush();
        var netStream = _input as NetworkStream;
        if (netStream == null)
            throw new NotImplementedException();
        var sslStream = new SslStream(netStream, true, Server.ValidateCertificate);
        sslStream.AuthenticateAsServer(Certificate);
        _input = sslStream;
        _output = sslStream;
    }

    public async Task<string> ReadLine()
        => await NetTools.ReadLineAsync(_input);

    public async Task WriteLine(string line)
        => await NetTools.WriteLineAsync(_output, line);

    internal async Task<byte[]> ReadData(int length)
        => await Task.FromResult(NetTools.ReadData(_input, length));

    public async Task Run()
    {
        await WriteLine("220 mail.axfab.com ESMTP");
        var badCommndCount = 0;
        var cmdLog = new StringBuilder();
        try
        {
            for (; ; )
            {
                var line = await ReadLine();
                string[] cmdParts = line.TrimStart().Split([' ']);
                string command = cmdParts[0].ToUpperInvariant().Trim();
                string argsText = line.Replace(cmdParts[0], "").Trim(); // TODO replace first only !

                bool knownCmd;
                ISmtpCommand? cmd;
                cmdLog.Append($"{command},");
                lock (_lock)
                {
                    knownCmd = _commands.TryGetValue(command, out cmd);
                }

                if (!knownCmd)
                {
                    await WriteLine("500 command unrecognized");
                    Console.WriteLine($"Unknown command: {line}");
                    if (++badCommndCount > Server.MaxBadCommands)
                    {
                        await WriteLine("421 Too many bad commands, closing transmission channel");
                        return;
                    }
                    continue;
                }

                if (!await cmd.Exec(this, argsText))
                    return;
            }
        }
        catch (TimeoutException ex)
        {
            await WriteLine("421 Session timeout, closing transmission channel");
        }
        finally
        {
            Console.WriteLine($"Session: {Remote} {cmdLog}");
        }
    }

    public static IEnumerable<KeyValuePair<string, string>> ParseParams(string source, params string[] expressions)
    {
        source = source.Trim();
        foreach (string exp in expressions)
        {
            var reg = new Regex(exp, RegexOptions.IgnoreCase);
            var mat = reg.Match(source);
            if (mat.Success)
            {
                yield return new KeyValuePair<string, string>(mat.Result("${param}").Trim().ToUpper(), mat.Result("${value}").Trim());
                source = source.Replace(mat.ToString(), "").Trim();
            }
        }

        if (!string.IsNullOrWhiteSpace(source))
            yield return new KeyValuePair<string, string>("UNPARSED", source.Trim());
    }

    internal void ChunkPush(byte[] value)
    {
        _chunkBuffer ??= new MemoryStream();
        _chunkBuffer.Write(value, 0, value.Length);
    }

    internal Stream Chunk()
    {
        var mem = _chunkBuffer;
        if (mem != null)
            mem.Seek(0, SeekOrigin.Begin);
        _chunkBuffer = null;
        return mem;
    }
}
