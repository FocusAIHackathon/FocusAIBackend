using System.Security.Cryptography.X509Certificates;
using FocusAI.Backend.Models;
using JWT;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Serializers;

namespace FocusAI.Backend.Models {

    public class Auth {
        /// <summary>
        /// This function takes in a user object which has only username and passowrd
        /// and returns JWT token for that user
        /// </summary>
        /// <param name="u">User object that has username and password</param>
        /// <returns></returns>
        public static string issueJWT(LoginRequest lr) {

            // dbUser is the full user object
            
            // User dbUser = User.getUser(u.username, u.password);


            var payload = new Dictionary<string, object>
            {
                { "exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()},
                { "username", "gsk" },
                { "user_fullname", "Gaurav Kalele" }
            };

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, "sampleKey");
            return token;
        }

    }
}
