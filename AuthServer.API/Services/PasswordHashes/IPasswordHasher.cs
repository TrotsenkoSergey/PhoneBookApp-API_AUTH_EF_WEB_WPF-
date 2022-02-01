namespace AuthServer.API.Services.PasswordHashes
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
    }
}
