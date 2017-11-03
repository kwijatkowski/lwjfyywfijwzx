using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Exchange.Kraken
{
    public class PrivateApiConnector
    {
        //documentation
        //https://www.kraken.com/help/api#general-usage

        private readonly string _publicKey;
        private readonly string _privateKey;

        private readonly string _baseAddress;

        public PrivateApiConnector(string baseAddress, string publicKey, string privateKey)
        {
            _baseAddress = baseAddress;
            _publicKey = publicKey;
            _privateKey = privateKey;
        }

        public async void NotWorkingConnect()
        {
            string urlPath = "";
            string props = "";

            // generate a 64 bit nonce using a timestamp at tick resolution
            Int64 nonce = DateTime.Now.Ticks;
            props = "nonce=" + nonce + props;

            HttpContent content = new StringContent("test");

            byte[] privateKeyBytes = Convert.FromBase64String(_privateKey);

            var nonceAndProperties = nonce + Convert.ToChar(0) + props;

            var pathBytes = Encoding.UTF8.GetBytes(urlPath);
            var hash256Bytes = sha256_hash(nonceAndProperties);
            var z = new byte[pathBytes.Count() + hash256Bytes.Count()];
            pathBytes.CopyTo(z, 0);
            hash256Bytes.CopyTo(z, pathBytes.Count());

            var signature = Convert.ToBase64String( hmacsha512(privateKeyBytes, z) );

            content.Headers.Add("API-Key", _publicKey);
            content.Headers.Add("API-Sign", signature);

            using (HttpClient client = new HttpClient())
            {
                await client.PostAsync(urlPath, content);
            }

        }

        private byte[] sha256_hash(String value)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;

                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                return result;
            }
        }

        private byte[] hmacsha512(byte[] keyByte, byte[] messageBytes)
        {
            using (var hmacsha512 = new HMACSHA512(keyByte))
            {
                Byte[] result = hmacsha512.ComputeHash(messageBytes);
                return result;

            }
        }
    }
}
