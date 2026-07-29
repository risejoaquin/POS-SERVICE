const fs = require('fs');

let file = 'PosServer/Program.cs';
let content = fs.readFileSync(file, 'utf8');

if (!content.includes('ALTER TABLE \\"OrderItem\\" ADD COLUMN \\"Notes\\"')) {
    content = content.replace(
        'dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Orders\\" ADD COLUMN \\"AuthorizedBy\\" text DEFAULT \'\';");',
        'dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Orders\\" ADD COLUMN \\"AuthorizedBy\\" text DEFAULT \'\';");\n            dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"OrderItem\\" ADD COLUMN \\"Notes\\" text DEFAULT \'\';");\n            dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"OrderItem\\" ADD COLUMN \\"Discount\\" numeric DEFAULT 0;");'
    );
    fs.writeFileSync(file, content);
}
