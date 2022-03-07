using System.Security.Cryptography;

namespace BiblioMit.Extensions
{
    public static class Hash
    {
        public static async Task<string> Get512Async(Uri url)
        {
            using HttpClient client = new();
            Stream stream = await client.GetStreamAsync(url).ConfigureAwait(false);
            using SHA512 sha = SHA512.Create();
            byte[] checksum = sha.ComputeHash(stream);
            await stream.DisposeAsync().ConfigureAwait(false);
            Thread.Sleep(200);
            return "sha512-" + Convert.ToBase64String(checksum);
        }
        public static string Get512Local(string file, int bufferSize = 1_000_000)
        {
            using BufferedStream stream = new(File.OpenRead(file), bufferSize);
            using SHA512 sha = SHA512.Create();
            byte[] checksum = sha.ComputeHash(stream);
            return "sha512-" + Convert.ToBase64String(checksum);
        }
        public static string Nonce()
        {
            //Allocate a buffer
            byte[] ByteArray = new byte[20];
            //Generate a cryptographically random set of bytes
            using RandomNumberGenerator Rnd = RandomNumberGenerator.Create();
            Rnd.GetBytes(ByteArray);
            //Base64 encode and then return
            return Convert.ToBase64String(ByteArray);
        }
    }
}
