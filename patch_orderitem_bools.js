const fs = require('fs');

let file = 'PosCore/Models/OrderItem.cs';
let content = fs.readFileSync(file, 'utf8');

if (!content.includes('HasNotes')) {
    content = content.replace(
        'public string Notes { get; set; } = string.Empty;',
        'public string Notes { get; set; } = string.Empty;\n    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);\n    public bool HasDiscount => Discount > 0;'
    );
    fs.writeFileSync(file, content);
}
