// using System.Security.Cryptography;

// public class UserAuth {
//     public int hashPass {get; set;}

//     public String username {get; set;}

//     public String token {get; set;}

//     public static String hash(String s) {
//         StringBuilder sb = new StringBuilder();
//         foreach (byte b in GetHash(s)) {
//             sb.append(b.ToString("X2"));
//         }

//         return sb.toString();
//     }

//     private static byte[] GetHash(string inputString) {
//         using (HashAlgorithm algorithm = SHA256.Create())
//             return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
//     }
    
// }