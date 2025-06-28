namespace XCoreNet.Base.Infrastructure.Security
{
    public interface IHashingService
    {
        string CreateSHA256(string input);
    }
}
