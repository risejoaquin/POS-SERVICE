sed -i 's/existingItem.SubTotal = existingItem.Quantity \* existingItem.Price;//g' PosCore/Views/MainWindow.xaml.cs
sed -i 's/Name = product.Name,/Product = product,/g' PosCore/Views/MainWindow.xaml.cs
sed -i 's/Price = product.Price,/UnitPrice = product.Price,/g' PosCore/Views/MainWindow.xaml.cs
sed -i '/SubTotal = product.Price/d' PosCore/Views/MainWindow.xaml.cs
