namespace PosCore.Models;

public class AppSettings
{
    public ApiSettings ApiSettings { get; set; } = new();
    public DatabaseSettings DatabaseSettings { get; set; } = new();
    public WhiteLabelSettings WhiteLabel { get; set; } = new();
    public ModuleSettings Modules { get; set; } = new();
    public TenantSettings Tenant { get; set; } = new();
}

public class TenantSettings
{
    public string CurrentTenantId { get; set; } = "TENANT_001";
}

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
}

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}

public class WhiteLabelSettings
{
    public string CompanyName { get; set; } = "Default POS";
    public string PrimaryColor { get; set; } = "#FF007ACC";
    public string LogoPath { get; set; } = string.Empty;
}

public class ModuleSettings
{
    public bool EnableTableManagement { get; set; }
    public bool EnableInventoryControl { get; set; }
}
