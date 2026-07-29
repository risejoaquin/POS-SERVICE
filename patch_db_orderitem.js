const fs = require('fs');

let file = 'PosCore/App.xaml.cs';
let content = fs.readFileSync(file, 'utf8');

if (!content.includes('ALTER TABLE OrderItem ADD COLUMN Notes')) {
    content = content.replace(
        'try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Orders ADD COLUMN AuthorizedBy TEXT NOT NULL DEFAULT \'\';"); } catch { }',
        'try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE Orders ADD COLUMN AuthorizedBy TEXT NOT NULL DEFAULT \'\';"); } catch { }\n                    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE OrderItem ADD COLUMN Notes TEXT NOT NULL DEFAULT \'\';"); } catch { }\n                    try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE OrderItem ADD COLUMN Discount TEXT NOT NULL DEFAULT \'0.0\';"); } catch { }'
    );
    fs.writeFileSync(file, content);
}
