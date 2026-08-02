using System;

namespace PosBuilder
{
    public static class SqlGenerator
    {
        private static string EscapeSql(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Replace("'", "''");
        }

        public static string GenerateTenantSql(string storeName, string tenantId, string adminUser, string adminPin, string empUser, string empPin)
        {
            string safeStoreName = EscapeSql(storeName);
            string safeTenantId = EscapeSql(tenantId);
            string safeAdminUser = EscapeSql(adminUser);
            string safeAdminPin = EscapeSql(adminPin);
            string safeEmpUser = EscapeSql(empUser);
            string safeEmpPin = EscapeSql(empPin);

            return $@"-- Initial users for {safeStoreName} ({safeTenantId})
INSERT INTO ""Users"" (""Username"", ""PasswordHash"", ""Role"", ""TenantId"") VALUES 
('{safeAdminUser}', crypt('{safeAdminPin}', gen_salt('bf')), 'Admin', '{safeTenantId}'),
('{safeEmpUser}', crypt('{safeEmpPin}', gen_salt('bf')), 'Cajero', '{safeTenantId}')
ON CONFLICT DO NOTHING;
";
        }
    }
}
