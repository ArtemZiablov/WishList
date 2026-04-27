namespace WishListApp.Interfaces;

public interface IAuthTokenProcessor
{
    // Returns the JWT string and when it expires
    (string jwtToken, DateTime expiresAtUtc) GenerateJwtToken(Models.User user);

    // Generates a cryptographically random refresh token string
    string GenerateRefreshToken();

    // Writes a token into an HttpOnly cookie — JavaScript cannot read this
    void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token, DateTime expiresAtUtc);
}