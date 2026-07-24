namespace PosCore.Services;

public class SessionManager
{
    public string CurrentTenantId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
}
