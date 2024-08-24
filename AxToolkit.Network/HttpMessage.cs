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
using System.Text;

namespace AxToolkit.Network;

public class HttpMessage
{
    public HttpMethod Method { get; set; }
    public string Path { get; private set; }
    public Uri Uri { get; set; }
    public int Status { get; set; }
    public string StatusMessage { get; set; }
    public bool KeepAlive { get; set; }
    public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
    public byte[] BodyContent { get; set; }
    public List<byte[]> ChunkedBody { get; private set; }

    public void Send(Stream stream) => Send(stream, null, true);
    public void Send(Stream stream, Encoding? encoding) => Send(stream, encoding, true);
    public void Send(Stream stream, Encoding? encoding, bool writeContent)
    {
        if (encoding == null)
            encoding = Encoding.UTF8;

        if (Method == HttpMethod.None)
            stream.Write(encoding.GetBytes($"HTTP/1.1 {Status} {StatusMessage}\r\n"));
        else
            stream.Write(encoding.GetBytes($"{Method} {Uri.PathAndQuery} HTTP/1.1\r\n"));

        //if ((BodyContent?.Length ?? 0) > 0)
        //    Headers["Content-Length"] = BodyContent.Length.ToString();

        foreach (var header in Headers)
            stream.Write(encoding.GetBytes($"{header.Key}: {header.Value}\r\n"));

        stream.Write(encoding.GetBytes("\r\n"));
        stream.Flush();

        if ((BodyContent?.Length ?? 0) > 0 && writeContent)
        {
            stream.Write(BodyContent);
            stream.Flush();
        }
        else if (ChunkedBody != null)
        {
            foreach (var chunk in ChunkedBody)
            {
                stream.Write(encoding.GetBytes($"{chunk.Length:x}\r\n"));
                stream.Write(chunk);
                stream.Write(encoding.GetBytes("\r\n"));
            }
            stream.Write(encoding.GetBytes($"0\r\n"));
            stream.Write(encoding.GetBytes("\r\n"));
            stream.Flush();
        }
    }

    public override string ToString()
    {
        using var mem = new MemoryStream();
        Send(mem, null, false);
        mem.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(mem);
        return reader.ReadToEnd();
    }

    public static HttpMessage? Read(Stream stream) => Read(stream, false);
    
    private static HttpMessage ParseHttpRequest(Stream stream)
    {
        var line = NetTools.ReadLine(stream);
        if (string.IsNullOrWhiteSpace(line))
            return null;
        var top = line.Split(' ');
        if (top.Length < 2)
            return null;
        var request = new HttpMessage();
        if (top[0] == "HTTP/1.0" || top[0] == "HTTP/1.1")
        {
            if (!int.TryParse(top[1], out var status) || status < 100 || status >= 600)
                return null;
            request.Status = status;
            request.StatusMessage = line.Substring(13).Trim();
        }
        else if (top[2] == "HTTP/1.0" || top[2] == "HTTP/1.1")
        {
            if (top.Length != 3 || !Enum.TryParse<HttpMethod>(top[0], out var method) || method == HttpMethod.None)
                return null;
            request.Method = method;
            request.Path = top[1];
        }
        return request;
    }

    private static bool ParseHttpHeaders(Stream stream, HttpMessage request)
    {
        for (; ; )
        {
            var line = NetTools.ReadLine(stream);
            if (string.IsNullOrWhiteSpace(line))
                return true;
            int idx = line.IndexOf(':');
            if (idx <= 0)
                return false;
            request.Headers[line.Substring(0, idx)] = line.Substring(idx + 1).Trim();
        }
    }

    private static bool ReadBody(Stream stream, HttpMessage request, int contentLength)
    {
        request.BodyContent = NetTools.ReadData(stream, contentLength);
        // TODO -- if (request.BodyContent.Length < contentLength)
        return true;
    }
    private static bool ReadChunkedBody(Stream stream, HttpMessage request)
    {
        request.ChunkedBody = new List<byte[]>();
        for (; ; )
        {
            var line = NetTools.ReadLine(stream);
            var len = Convert.ToInt32(line.Split(' ')[0], 16);
            if (len == 0)
            {
                for (; ; )
                {
                    line = NetTools.ReadLine(stream);
                    if (string.IsNullOrWhiteSpace(line))
                        return true;
                    int idx = line.IndexOf(':');
                    if (idx <= 0)
                        return false;
                    // request.Headers[line.Substring(0, idx)] = line.Substring(idx + 1).Trim();
                }
            }
            request.ChunkedBody.Add(NetTools.ReadData(stream, len));
            line = NetTools.ReadLine(stream);
            if (line != "")
                return false;
        }
    }

    public static HttpMessage? Read(Stream stream, bool secured)
    {
        // Parse HTTP Request
        var request = ParseHttpRequest(stream);

        // Parse HTTP Header
        if (!ParseHttpHeaders(stream, request))
            return null;

        // Read HTTP Body
        if (request.Headers.TryGetValue("Content-Length", out var lengthStr) && int.TryParse(lengthStr, out var contentLength))
        {
            if (!ReadBody(stream, request, contentLength))
                return null;
        }
        else if (request.Headers.TryGetValue("Transfer-Encoding", out var bodyEnc))
        {
            if (bodyEnc == "chunked")
            {
                if (!ReadChunkedBody(stream, request))
                    return null;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        
        if (request.Method != HttpMethod.None)
        {
            var hostname = request.Headers.TryGetValue("Host", out var host) ? host : "localhost";
            var protocol = secured ? "https" : "http";
            request.Uri = new Uri($"{protocol}://{hostname}{request.Path}");
        }
        return request;
    }
}