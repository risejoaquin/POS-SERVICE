with open("PosCore/App.xaml.cs", "r", encoding="utf-8") as f:
    content = f.read()

import re

old_code = """        // Aplicar migraciones y Backup
        using (var scope = ServiceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PosDbContext>();
            dbContext.Database.EnsureCreated();"""

new_code = """        // Aplicar migraciones y Backup
        using (var scope = ServiceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PosDbContext>();
            
            // Check if Products table exists, if not, the database is outdated and we should delete it
            try {
                dbContext.Database.ExecuteSqlRaw("SELECT 1 FROM Products LIMIT 1;");
            } catch {
                Serilog.Log.Warning("Products table not found, deleting and recreating database...");
                dbContext.Database.EnsureDeleted();
            }
            
            dbContext.Database.EnsureCreated();"""

if old_code in content:
    content = content.replace(old_code, new_code)
    with open("PosCore/App.xaml.cs", "w", encoding="utf-8") as f:
        f.write(content)
    print("Replaced!")
else:
    print("Could not find old code.")
