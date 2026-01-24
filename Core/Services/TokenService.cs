using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PortfolioPro.Interfaces;
using PortfolioPro.Core.Models;

namespace PortfolioPro.Services;
/** $ ya can't touch this $ **/
/// <summary>
/// Service responsible for generating sec JSON Web Tokens (JWT) for user authorization.
/// </summary>
public class TokenService(IConfiguration config) : ITokenService
{
    /// <summary>
    /// Creates a signed JWT containing 
    /// user identity claim, valid for 7 days.
    /// </summary>
    /// <param name="user">The user entity whose information will be encoded into the token.</param>
    /// <returns>A serialized JWT string.</returns>
    public string CreateToken(User user)
    {
        // Converts the secret key from appsettings into a byte array cryptographic use.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

        // Sets up the digital signature using the secret key.
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        // Defines the specific pieces of user claims to be stored inside the token.
        var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject claim (Unique ID).
                new Claim(JwtRegisteredClaimNames.Email, user.Email), // Email claim.
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // Standard .NET name identifier.
            };

        // Configures the token's properties including identity, expiration, and security credentials.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),  // Attaches the user claims defined above.
            Expires = DateTime.UtcNow.AddDays(7),  // Sets the token to expire exactly one week from creation.
            SigningCredentials = creds,            // Signs the token to prevent tampering.
            Issuer = config["Jwt:Issuer"],         // Identifies the server that issued the token.
            Audience = config["Jwt:Audience"]      // Identifies the intended recipients of the token.
        };

        // Initializes the handler responsible for creating and writing JWT data.
        var tokenHandler = new JwtSecurityTokenHandler();

        // Generates the security token object based on the descriptor.
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // Serializes the token into it's final 3 part "header.payload.signature" string format.
        return tokenHandler.WriteToken(token);
    }
}