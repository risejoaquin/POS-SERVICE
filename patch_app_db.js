const fs = require('fs');
let content = fs.readFileSync('PosCore/App.xaml.cs', 'utf8');

content = content.replace(
    'try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE OutboxMessages ADD COLUMN RetryCount INTEGER NOT NULL DEFAULT 0;"); } catch { }',
    'try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE OutboxMessages ADD COLUMN RetryCount INTEGER NOT NULL DEFAULT 0;"); } catch { }\n                    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Orders ADD COLUMN ReturnReason TEXT NOT NULL DEFAULT \'\';"); } catch { }\n                    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Orders ADD COLUMN AuthorizedBy TEXT NOT NULL DEFAULT \'\';"); } catch { }'
);

fs.writeFileSync('PosCore/App.xaml.cs', content);
