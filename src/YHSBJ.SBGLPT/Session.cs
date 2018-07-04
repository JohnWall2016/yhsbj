using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Configuration;

namespace YHSBJ.SBGLPT
{
    public partial class Session : IDisposable
    {
        TcpClient _client;
        NetworkStream _stream;
        string _sessionId = "";
        string _encodeName = "GBK";
        
        public Session(string ip, int port)
        {
            _client = new TcpClient(ip, port);
            try
            {
                _stream = _client.GetStream();
            }
            catch (Exception ex)
            {
                _client.Close();
                throw ex;
            }

            IP = ip;
            Port = port;
        }

        public string IP { get; }
        public int Port { get; }
        public string Userid { get; set; }
        public string Password { get; set; }

        Encoding _enc = null;
        public Encoding Enc
        {
            get
            {
                if (_enc == null)
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    _enc = Encoding.GetEncoding(_encodeName);
                }
                return _enc;
            }
        }

        public string Url => $"{IP}:{Port}";

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }

        void Write(string content)
        {
            if (_stream == null)
                throw new ApplicationException("service is not connected");
            var data = Enc.GetBytes(content);
            _stream.Write(data, 0, data.Length);
        }

        (byte[], int) Read(int len)
        {
            if (_stream == null)
                throw new ApplicationException("service is not connected");
            var data = new Byte[len];
            var size = _stream.Read(data, 0, len);
            return (data, size);
        }

        string ReadLine()
        {
            if (_stream == null)
                throw new ApplicationException("service is not connected");
            using (var mem = new MemoryStream(512))
            {
                int c = 0, n = 0;
                while (true)
                {
                    c = _stream.ReadByte();
                    if (c == -1)
                        return Enc.GetString(mem.GetBuffer(), 0, (int)mem.Length);
                    if (c == 0xD)
                    {
                        n = _stream.ReadByte();
                        if (n == 0xA)
                            return Enc.GetString(mem.GetBuffer(), 0, (int)mem.Length);
                        else if (n == -1)
                        {
                            mem.WriteByte((byte)c);
                            return Enc.GetString(mem.GetBuffer(), 0, (int)mem.Length);
                        }
                        else
                        {
                            mem.WriteByte((byte)c);
                            mem.WriteByte((byte)n);
                        }
                    } else
                        mem.WriteByte((byte)c);
                }
            }
        }

        string ReadHeader()
        {
            var result = new StringBuilder(512);
            while (true)
            {
                var line = ReadLine();
                if (line == null || line == "") break;
                result.Append(line + "\n");
            }
            return result.ToString();
        }

        string ReadBody(string header = "")
        {
            using(var data = new MemoryStream(512))
            {
                if (header == "")
                    header = ReadHeader();
                if (Regex.IsMatch(header, "Transfer-Encoding: chunked"))
                {
                    while (true)
                    {
                        var len = Convert.ToInt32(ReadLine(), 16);
                        if (len <= 0)
                        {
                            ReadLine();
                            break;
                        }
                        while (true)
                        {
                            (var rec, var rlen) = Read(len);
                            data.Write(rec, 0, rlen);
                            len -= rlen;
                            if (len <= 0)
                            {
                                break;
                            }
                        }
                        ReadLine();
                    }
                }
                else
                {
                    var match = Regex.Match(header, @"Content-Length: (\d+)");
                    if (match.Length > 0)
                    {
                        var len = Convert.ToInt32(match.Groups[1].Value, 10);
                        while (len > 0)
                        {
                            (var rec, var rlen) = Read(len);
                            data.Write(rec, 0, rlen);
                            len -= rlen;
                        }
                    }
                    else
                        throw new ApplicationException("Unsupported transfer mode");
                }
                var result = Enc.GetString(data.GetBuffer(), 0, (int)data.Length);
                //Console.WriteLine($"\nReadBody: {result}");
                return result;
            }
        }

        string MakeSendContent(string content)
        {
            var result =
                "POST /sbzhpt/MainServlet HTTP/1.1\n" +
                "SOAPAction: mainservlet\n" +
                $"Content-Type: text/html;charset={_encodeName}\n" +
                $"Host: {Url}\n";
            
            if (_sessionId != "")
                result += $"Cookie: JSESSIONID={_sessionId}\n";
                
            result +=
                "Connection: Keep-Alive\n" +
                "Cache-Control: no-cache\n" +
                $"Content-Length: {Enc.GetByteCount(content)}\n";

            result += $"\n{content}";
            return result;
        }

        void Send(string serviceContent)
        {
            var send = MakeSendContent(serviceContent);
            //Console.WriteLine($"\nSend: {send}");
            Write(send);
        }

        public void SendInput(Action<Envelope<Input>> addParams)
        {
            var inEnv = new Envelope<Input>
            {
                Header = new Input("system"),
                Body  = new Input("business")
            };
            inEnv.Header.Params.Add("usr", Userid);
            inEnv.Header.Params.Add("pwd", Password);
            
            addParams(inEnv);
            Send(inEnv.ToString());
        }

        public string Get()
        {
            return ReadBody();
        }

        public Envelope<Output> GetOutput()
        {
            return Envelope<Output>.Load(Get());
        }
            
        public Envelope<Output> Login()
        {
            SendInput(inEnv => inEnv.Header.Params.Add("funid", "F00.00.00.00|192.168.1.110|PC-20170427DGON|00-05-0F-08-1A-34"));
            var header = ReadHeader();
            var match = Regex.Match(header, "Set-Cookie: JSESSIONID=(.+?);");
            if (match.Length > 0)
                _sessionId = match.Groups[1].Value;
            
            return Envelope<Output>.Load(ReadBody(header));
        }

        public void Logout()
        {
            //TODO
            //throw new NotImplementedException();
        }
    }
}
