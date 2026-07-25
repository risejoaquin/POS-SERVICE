const fs = require('fs');
let code = fs.readFileSync('PosCore/ViewModels/MainViewModel.cs', 'utf8');

const target = `            await _syncService.SyncDataAsync();
            localProducts = await _dbContext.Products.AsNoTracking().ToListAsync();
        }

        Products.Clear();`;

const replacement = `            await _syncService.SyncDataAsync();
            localProducts = await _dbContext.Products.AsNoTracking().ToListAsync();
            
            // Seed default products if still empty
            if (!localProducts.Any())
            {
                var dummyProducts = new System.Collections.Generic.List<PosCore.Models.Product>
                {
                    new PosCore.Models.Product { Name = "Coca Cola 600ml", Price = 1.50m, Barcode = "7501055300075", StockQuantity = 100 },
                    new PosCore.Models.Product { Name = "Gansito Marinela", Price = 1.20m, Barcode = "7501000142200", StockQuantity = 50 },
                    new PosCore.Models.Product { Name = "Sabritas Sal 40g", Price = 1.00m, Barcode = "7501011115545", StockQuantity = 75 },
                    new PosCore.Models.Product { Name = "Agua Ciel 1L", Price = 0.90m, Barcode = "7501055310883", StockQuantity = 120 }
                };
                
                _dbContext.Products.AddRange(dummyProducts);
                await _dbContext.SaveChangesAsync();
                
                localProducts = await _dbContext.Products.AsNoTracking().ToListAsync();
            }
        }

        Products.Clear();`;

code = code.replace(target, replacement);
fs.writeFileSync('PosCore/ViewModels/MainViewModel.cs', code);
