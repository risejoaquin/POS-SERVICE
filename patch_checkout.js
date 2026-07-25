const fs = require('fs');
let code = fs.readFileSync('PosCore/ViewModels/MainViewModel.cs', 'utf8');

code = code.replace(
    /product\.StockQuantity -= item\.Quantity;\s*}/g,
    "product.StockQuantity -= item.Quantity;\n                    item.Product = product;\n                }"
);

fs.writeFileSync('PosCore/ViewModels/MainViewModel.cs', code);
