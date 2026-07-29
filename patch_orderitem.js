const fs = require('fs');

let file = 'PosCore/Models/OrderItem.cs';
let content = fs.readFileSync(file, 'utf8');

if (!content.includes('Notes')) {
    content = content.replace(
        'public decimal SubTotal => Quantity * UnitPrice;',
        'public decimal Discount { get; set; } = 0;\n    public string Notes { get; set; } = string.Empty;\n    public decimal SubTotal => (Quantity * UnitPrice) - Discount;'
    );
    fs.writeFileSync(file, content);
}

let fileServer = 'PosServer/Models/OrderItem.cs';
if (fs.existsSync(fileServer)) {
    let contentServer = fs.readFileSync(fileServer, 'utf8');
    if (!contentServer.includes('Notes')) {
        contentServer = contentServer.replace(
            'public decimal SubTotal => Quantity * UnitPrice;',
            'public decimal Discount { get; set; } = 0;\n    public string Notes { get; set; } = string.Empty;\n    public decimal SubTotal => (Quantity * UnitPrice) - Discount;'
        );
        fs.writeFileSync(fileServer, contentServer);
    }
}
