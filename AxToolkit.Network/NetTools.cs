using System.Net.Sockets;
using System.Text;

namespace AxToolkit.Network
{
    public static class NetTools
    {
        public static string? ReadLine(Stream stream)
        {
            var sb = new StringBuilder();
            try
            {
                for (; ; )
                {
                    var rd = stream.ReadByte(); // TODO -- Timelimit
                    if (rd == '\n')
                        return sb.ToString();
                    if (rd >= 0x20 && rd < 0x80)
                        sb.Append((char)rd);
                }
            }
            catch
            {
                if (sb.Length == 0)
                    return null;
                throw;
            }
        }

        public static byte[] ReadData(Stream stream, int length)
        {
            var bytes = new byte[length];
            var offset = 0;
            while (offset < length)
            {
                var len = stream.Read(bytes, offset, length - offset); // TODO -- Timelimit
                offset += len;
                if (len <= 0)
                    throw new IOException("Unexpected error on reading operation");
            }
            return bytes;
        }

        internal static byte[]? ReadUntil(Stream stream, string terminator, long maxLength, string equal = null)
        {
            var data = new MemoryStream();

            var endingArr = Encoding.UTF8.GetBytes(terminator);
            var sackArr = new byte[endingArr.Length];

            var buf = new byte[500];
            int toRead = Math.Min(terminator.Length, equal.Length);
            while (true)
            {
                var len = stream.Read(buf, 0, toRead);
                if (len == 0)
                    return null;

                data.Write(buf, 0, len);
                if (data.Length > maxLength)
                    return null;

                if (equal != null && data.Length == equal.Length)
                {
                    data.Flush();
                    var rd = new StreamReader(data);
                    data.Seek(0, SeekOrigin.Begin);
                    var content = rd.ReadToEnd();
                    if (content == equal)
                        return Encoding.ASCII.GetBytes(equal);
                }

                Array.Copy(sackArr, sackArr.Length - len, sackArr, 0, len);
                Array.Copy(buf, 0, sackArr, sackArr.Length - len, len);
                int idx = Array.IndexOf(sackArr, endingArr[0]);
                if (idx < 0)
                    toRead = terminator.Length;
                else if (idx > 0)
                    toRead = idx;
                else
                {
                    toRead = 0;
                    for (int i = 1; i < sackArr.Length; ++i)
                    {
                        if (sackArr[i] != endingArr[i])
                        {
                            toRead = 1;
                            break;
                        }
                    }
                    if (toRead == 0)
                    {
                        data.Flush();
                        var bytes = new byte[data.Length];
                        data.Seek(0, SeekOrigin.Begin);
                        data.Read(bytes, 0, bytes.Length);
                        return bytes;
                    }
                }
            }
        }

        public static void WriteLine(Stream stream, string line)
        {
            if (!line.EndsWith("\r\n"))
                line += "\r\n";
            var data = Encoding.UTF8.GetBytes(line);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        internal static Task<string?> ReadLineAsync(Stream input)
        {
            var res = ReadLine(input);
            return Task.FromResult(res);
        }

        internal static Task WriteLineAsync(Stream output, string line)
        {
            WriteLine(output, line);
            return Task.CompletedTask;
        }
    }

}
