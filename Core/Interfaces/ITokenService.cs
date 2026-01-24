using PortfolioPro.Core.Models;
/** This is an interface for
managing JWT token creation.
**/
namespace PortfolioPro.Interfaces;

/// <summary>
/// Creates an encrypted JWT token
/// contianing the user's identity
/// claims.
/// </summary>
public interface ITokenService
{
    string CreateToken(User user);
}