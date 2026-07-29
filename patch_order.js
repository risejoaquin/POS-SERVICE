const fs = require('fs');

function patchModel(file) {
    let content = fs.readFileSync(file, 'utf8');
    if (!content.includes('ReturnReason')) {
        content = content.replace(
            'public bool IsReturned { get; set; } = false;',
            'public bool IsReturned { get; set; } = false;\n    public string ReturnReason { get; set; } = string.Empty;\n    public string AuthorizedBy { get; set; } = string.Empty;'
        );
        fs.writeFileSync(file, content);
    }
}

patchModel('PosCore/Models/Order.cs');
patchModel('PosServer/Models/Order.cs');
