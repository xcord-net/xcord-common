namespace Xcord;

public interface ICurrentUserService
{
    Result<long> GetCurrentUserId();
}
