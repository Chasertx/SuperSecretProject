using PortfolioPro.Core.Models;

namespace PortfolioPro.Interfaces;

public interface ITokenService
{
    // Rule: Create a secure, digital "key card" (token) that proves who the user is while they browse the site
    string CreateToken(User user);
}