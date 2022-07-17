using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Identity_UI.Models;

public class PasswordHasher
{
    
    public  static string GetHashPassword(string password)
    {
        var inputBytes = Encoding.UTF8.GetBytes(password);
        var sha = System.Security.Cryptography.SHA256.Create();
        var outPutBytes = sha.ComputeHash(inputBytes);
        var result = Convert.ToBase64String(outPutBytes);
        sha.Dispose();
        return result;
    }
}