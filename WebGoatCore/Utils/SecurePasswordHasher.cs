using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;


namespace WebGoatCore.Utils
{
    public class SecurePasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class
    {
        public string HashPassword(TUser user, string password)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hashSh1 = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));

                // declare stringbuilder
                var sb = new StringBuilder(hashSh1.Length * 2);

                // computing hashSh1
                foreach (byte b in hashSh1)
                {
                    // "x2"
                    sb.Append(b.ToString("X2").ToLower());
                }
                return sb.ToString();
            }
            throw new System.ArgumentException("Unable to hash provided password");
        }

        public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
        {
            string newHashedPassword = HashPassword(user, providedPassword);
            if (hashedPassword.Equals(newHashedPassword))
                return PasswordVerificationResult.Success;
            else return PasswordVerificationResult.Failed;
        }
    }
}
