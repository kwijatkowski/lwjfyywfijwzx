using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Generic;

namespace Exchange.BitBay
{
    public class PrivateApiConnector
    {
        private readonly string _publicKey;
        private readonly string _privateKey;

        private const string customNTPserver = "aaa.com";
        private string _baseAddress;

        public PrivateApiConnector(string baseAddress, string publicKey, string privateKey)
        {
            _baseAddress = baseAddress;
            _publicKey = publicKey;
            _privateKey = privateKey;
        }

        public async Task<string> Info(string ntpServer = customNTPserver)
        {
            DateTime now = GetNetworkTime(ntpServer);
            Int32 unixTimestamp = (Int32)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            string moment = unixTimestamp.ToString();

            HttpClient client = new HttpClient(new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Automatic
            });

            var encoding = Encoding.UTF8;
            byte[] keyByte = encoding.GetBytes(_privateKey);

            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

            queryString["method"] = "info";
            queryString["moment"] = unixTimestamp.ToString();

            string postMessage = queryString.ToString();

            HMACSHA512 shaM = new HMACSHA512(keyByte);
            shaM.ComputeHash(encoding.GetBytes(postMessage));

            byte[] result = shaM.Hash;
            string hash = ByteToString(result);

            HttpContent content = new StringContent(postMessage);
            content.Headers.Add("API-Key", _publicKey);
            content.Headers.Add("API-Hash", hash);

            string requestAddress = _baseAddress;

            HttpResponseMessage response = await client.PostAsync(requestAddress, content);

            Stream s = await response.Content.ReadAsStreamAsync();
            var header = response.Headers;
            MemoryStream ms = new MemoryStream();
            s.CopyTo(ms);
            string returnMsg = encoding.GetString(ms.ToArray());
            return returnMsg;
        }

        public async Task<string> GetData(Dictionary<string,string> parameters, string ntpServer = customNTPserver)
        {
            DateTime now = GetNetworkTime(ntpServer);
            Int32 unixTimestamp = (Int32)(now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            string moment = unixTimestamp.ToString();

            //todo: think if need to automaticaly close connection
            HttpClient client = new HttpClient(new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Automatic
            });

            var encoding = Encoding.UTF8;
            byte[] keyByte = encoding.GetBytes(_privateKey);

            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);

            //queryString["method"] = method;
            queryString["moment"] = unixTimestamp.ToString();

            foreach (KeyValuePair<string,string> kvp in parameters)
                queryString[kvp.Key] = kvp.Value;

            string postMessage = queryString.ToString();

            HMACSHA512 shaM = new HMACSHA512(keyByte);
            shaM.ComputeHash(encoding.GetBytes(postMessage));

            byte[] result = shaM.Hash;
            string hash = ByteToString(result);

            HttpContent content = new StringContent(postMessage);
            content.Headers.Add("API-Key", _publicKey);
            content.Headers.Add("API-Hash", hash);

            string requestAddress = _baseAddress;

            HttpResponseMessage response = await client.PostAsync(requestAddress, content);

            Stream s = await response.Content.ReadAsStreamAsync();
            var header = response.Headers;
            MemoryStream ms = new MemoryStream();
            s.CopyTo(ms);
            string returnMsg = encoding.GetString(ms.ToArray());
            return returnMsg;
        }


        private string ByteToString(byte[] buff)
        {
            string sbinary = "";
            for (int i = 0; i < buff.Length; i++)
                sbinary += buff[i].ToString("x2"); /* hex format */
            return sbinary;
        }


        public DateTime GetNetworkTime(string ntpServerAddress)
        {
            var ntpData = new byte[48];
            ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

            string ntpServer = ntpServerAddress == null ? "pool.ntp.org" : ntpServerAddress;
            var addresses = Dns.GetHostEntry(ntpServer).AddressList;
            IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], 123);

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.ReceiveTimeout = 1000;
            socket.Connect(ipEndPoint);
            socket.Send(ntpData);
            socket.Receive(ntpData);
            socket.Close();

            ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
            ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

            return networkDateTime;
        }
    }
}