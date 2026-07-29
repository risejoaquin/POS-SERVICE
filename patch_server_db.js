const fs = require('fs');
let content = fs.readFileSync('PosServer/Program.cs', 'utf8');

content = content.replace(
    '        creator.CreateTables();\n    }\n    catch',
    '        creator.CreateTables();\n    }\n    catch\n    {\n        // Tables already exist, try to add new columns for updates\n        try\n        {\n            dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Orders\\" ADD COLUMN \\"ReturnReason\\" text DEFAULT \'\';");\n            dbContext.Database.ExecuteSqlRaw("ALTER TABLE \\"Orders\\" ADD COLUMN \\"AuthorizedBy\\" text DEFAULT \'\';");\n        }\n        catch { /* Columns probably already exist */ }\n    }\n    catch'
);

fs.writeFileSync('PosServer/Program.cs', content);
