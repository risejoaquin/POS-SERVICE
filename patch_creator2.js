const fs = require('fs');
let code = fs.readFileSync('PosCore/App.xaml.cs', 'utf8');

const target = `            try 
            {
                var creator = Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacadeExtensions.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>(dbContext.Database);
                // We'll just EnsureCreated because it's safer if there are no tables.
                // Wait, if __EFMigrationsHistory is there, EnsureCreated won't work.
                
                // If it fails because tables exist, we catch it.
                try {
                    dbContext.Database.EnsureCreated();
                    var relCreator = dbContext.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>();
                    relCreator.CreateTables();
                } catch { }
            }`;

const replacement = `            try 
            {
                try {
                    dbContext.Database.EnsureCreated();
                    var relCreator = dbContext.Database.GetService<IRelationalDatabaseCreator>();
                    relCreator.CreateTables();
                } catch { }
            }`;

code = code.replace(target, replacement);
fs.writeFileSync('PosCore/App.xaml.cs', code);
