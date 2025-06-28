using System.Security.Cryptography;
using System.Text;

namespace XCoreNet.Base.Infrastructure.Security
{
    public class HashingService : IHashingService
    {
        public string CreateSHA256(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));

                return sb.ToString();
            }
        }
    }
}
