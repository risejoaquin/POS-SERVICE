using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PosBuilder.Models;

namespace PosBuilder
{
    public class ConfigurationGenerator
    {
        public string GenerateAppSettings(ConfigModel model)
        {
            var config = new
            {
                ApiSettings = new
                {
                    BaseUrl = model.ApiBaseUrl
                },
                DatabaseSettings = new
                {
                    ConnectionString = "Data Source=pos_local.db;Default Timeout=30;"
                },
                WhiteLabel = new
                {
                    CompanyName = model.CompanyName,
                    PrimaryColor = model.PrimaryColor,
                    LogoPath = model.LogoPath
                },
                Tenant = new
                {
                    CurrentTenantId = model.TenantId
                },
                Security = new
                {
                    ManagerPin = "" // Dejar vacío intencionalmente
                }
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(config, options);
        }

        public string GenerateEnvFile(ConfigModel model)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# Environment: {model.Environment}");
            sb.AppendLine($"PORT=3000");
            sb.AppendLine($"DB_TYPE={model.DbType}");
            sb.AppendLine($"DB_HOST={model.DbHost}");
            sb.AppendLine($"DB_PORT={model.DbPort}");
            sb.AppendLine($"DB_USER={model.DbUser}");
            sb.AppendLine($"DB_PASSWORD={model.DbPassword}");
            sb.AppendLine($"DB_NAME={model.DbName}");
            sb.AppendLine($"JWT_KEY={model.JwtSecret}");
            sb.AppendLine($"ADMIN_USER={model.AdminUser}");
            sb.AppendLine($"ADMIN_PASSWORD={model.AdminPassword}");
            sb.AppendLine($"EMP_USER={model.EmployeeUser}");
            sb.AppendLine($"EMP_PASSWORD={model.EmployeePassword}");
            sb.AppendLine($"TENANT_ID={model.TenantId}");
            sb.AppendLine($"BUSINESS_TYPE={model.BusinessType}");
            
            return sb.ToString();
        }


        public string GenerateServerAppSettings(ConfigModel model)
        {
            string connString = "";
            if (model.DbType.ToLower().Contains("sqlite")) {
                connString = "Data Source=posdb.sqlite";
            } else {
                connString = $"Host={model.DbHost};Port={model.DbPort};Database={model.DbName};Username={model.DbUser};Password={model.DbPassword};SSL Mode=Require;Trust Server Certificate=True";
            }
            
            var config = new
            {
                Logging = new { LogLevel = new { Default = "Information", Microsoft_AspNetCore = "Warning" } },
                AllowedHosts = "*",
                ConnectionStrings = new { DefaultConnection = connString },
                Jwt = new { Key = model.JwtSecret, Issuer = "PosServer", Audience = "PosClient" },
                ADMIN_USER = model.AdminUser,
                ADMIN_PASSWORD = model.AdminPassword,
                EMP_USER = model.EmployeeUser,
                EMP_PASSWORD = model.EmployeePassword,
                TENANT_ID = model.TenantId,
                BUSINESS_TYPE = model.BusinessType
            };
            
            return System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }).Replace("Microsoft_AspNetCore", "Microsoft.AspNetCore");
        }
        public string GenerateSqlScript(ConfigModel model)
        {
            return SqlGenerator.GenerateTenantSql(
                model.CompanyName, 
                model.TenantId, 
                model.AdminUser, 
                model.AdminPassword, 
                model.EmployeeUser, 
                model.EmployeePassword);
        }

        public async Task<bool> WriteWithIntegrityValidationAsync(string path, string content, int retries = 3)
        {
            int attempt = 0;
            while (attempt < retries)
            {
                try
                {
                    var dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);

                    await File.WriteAllTextAsync(path, content, Encoding.UTF8);
                    
                    // Validate hash
                    string writtenHash = ComputeSha256(await File.ReadAllTextAsync(path, Encoding.UTF8));
                    string expectedHash = ComputeSha256(content);
                    
                    if (writtenHash == expectedHash)
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    // Log or handle exception if needed
                }
                attempt++;
            }
            return false;
        }

        private string ComputeSha256(string text)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
