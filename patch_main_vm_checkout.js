const fs = require('fs');
let code = fs.readFileSync('PosCore/ViewModels/MainViewModel.cs', 'utf8');

const target = `                var product = await _dbContext.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    if (product.StockQuantity < item.Quantity)
                    {
                        System.Windows.MessageBox.Show($"Stock insuficiente para {product.Name}. Compra no procesada.", "Aviso de Stock", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return; // Cancela el proceso
                    }
                    product.StockQuantity -= item.Quantity;
                }`;

const replacement = `                var product = await _dbContext.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    if (product.StockQuantity < item.Quantity)
                    {
                        System.Windows.MessageBox.Show($"Stock insuficiente para {product.Name}. Compra no procesada.", "Aviso de Stock", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return; // Cancela el proceso
                    }
                    product.StockQuantity -= item.Quantity;
                    item.Product = product; // EF Core will track this instance
                }`;

code = code.replace(target, replacement);

const targetCartAdd = `        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            Cart.Add(new OrderItem
            {
                ProductId = product.Id,
                Product = product,
                UnitPrice = product.Price,
                Quantity = 1
            });
        }`;

const replacementCartAdd = `        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            Cart.Add(new OrderItem
            {
                ProductId = product.Id,
                Product = null, // Do not set Product navigation here to avoid tracking conflicts later
                UnitPrice = product.Price,
                Quantity = 1,
                // store Name/Barcode for display since Product is null
                ProductNameDisplay = product.Name 
            });
        }`;

// Wait, does OrderItem have ProductNameDisplay? No.
// But the UI binds to Product.Name. If I set Product to null, the UI will not show the name in the cart!
// Let's check MainWindow.xaml to see what it binds to.
