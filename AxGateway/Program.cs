﻿using AxToolkit.Network;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AxGateway
{
    public class Program : HttpGateway
    {
        private string _certificat;
        private string _password;
        private string _index;
        private readonly object _x509Lock = new object();
        private X509Certificate _x509Certificate = null;

        public static void Main(string[] args)
        {
            var gateway = new Program();
            
            var lines = File.ReadAllLines("gateway.config")
                .Where(x => !(string.IsNullOrWhiteSpace(x) || x.Trim().StartsWith("#")))
                .Select(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Where(x => x.Length == 2);

            foreach (var line in lines)
            {
                if (line[0] == "SECURE")
                    gateway._certificat = line[1];
                else if (line[0] == "PASSWORD")
                    gateway._password = line[1];
                else if (line[0] == "INDEX")
                    gateway._index = line[1];
                else
                {
                    gateway.AddService(line[1], int.Parse(line[0]));
                    Console.WriteLine($"Assign port {line[0]} for {line[1]}");
                }
            }

            if (File.Exists(gateway._certificat))
                Task.Run(() => gateway.Listen(443, true));
            gateway.Listen(80);
        }


        protected override bool OnIncoming(HttpMessage request)
        {
            if (request.Uri.AbsolutePath.StartsWith("/wp-") || request.Uri.AbsolutePath.EndsWith(".php"))
                return false; // Block most robots looking for wordpress or php access...
            return true;
        }

        protected override HttpMessage? OnError(Exception ex)
        {
            if (!File.Exists(_index))
                return null;
            var html = Encoding.UTF8.GetBytes(File.ReadAllText(_index));
            var res = new HttpMessage
            {
                Status = 200,
                StatusMessage = "Ok",
                KeepAlive = false,
                BodyContent = html,
            };
            res.Headers.Add("Content-Length", html.Length.ToString());
            res.Headers.Add("Content-Type", "text/html; charset=utf-8");
            return res;
        }


        public override X509Certificate? SelectCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate? remoteCertificate, string[] acceptableIssuers)
        {
            lock (_x509Lock)
            {
                return _x509Certificate ??= new X509Certificate(_certificat, _password);
            }
        }
    }
}
