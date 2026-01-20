using PortfolioPro.Core.Models;
/** This is an interface for
managing JWT token creation.
**/
namespace PortfolioPro.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
}