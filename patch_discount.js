const fs = require('fs');

// Patch MainViewModel.cs
let vm = fs.readFileSync('PosCore/ViewModels/MainViewModel.cs', 'utf8');

vm = vm.replace(
    'public partial class MainViewModel : ObservableObject\n{\n    private readonly PosDbContext _dbContext;',
    'public partial class MainViewModel : ObservableObject\n{\n    private readonly PosDbContext _dbContext;\n\n    [ObservableProperty]\n    private decimal _discountAmount = 0m;\n\n    [ObservableProperty]\n    private decimal _subTotal = 0m;'
);

vm = vm.replace(
    'private void UpdateTotal()\n    {\n        Total = Cart.Sum(i => i.SubTotal);\n    }',
    'private void UpdateTotal()\n    {\n        SubTotal = Cart.Sum(i => i.SubTotal);\n        // Simulate auto discount evaluation (e.g. 10% off for combo if more than 2 items)\n        if (Cart.Count >= 2)\n        {\n            DiscountAmount = SubTotal * 0.10m;\n        }\n        else\n        {\n            DiscountAmount = 0;\n        }\n        Total = SubTotal - DiscountAmount;\n    }'
);

fs.writeFileSync('PosCore/ViewModels/MainViewModel.cs', vm);

// Patch MainWindow.xaml
let view = fs.readFileSync('PosCore/Views/MainWindow.xaml', 'utf8');

view = view.replace(
    '                <!-- Totales -->\n                <Border Grid.Row="1" BorderBrush="LightGray" BorderThickness="0,1,0,0" Padding="0,10">\n                    <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">\n                        <TextBlock Text="Total:" FontSize="20" FontWeight="Bold" Margin="0,0,10,10" VerticalAlignment="Center"/>\n                        <TextBlock Text="{Binding Total, StringFormat=C}" FontSize="24" FontWeight="Bold" Foreground="#28A745" VerticalAlignment="Center"/>\n                    </StackPanel>\n                </Border>',
    `                <!-- Totales y Descuentos -->
                <Border Grid.Row="1" BorderBrush="LightGray" BorderThickness="0,1,0,0" Padding="0,10">
                    <StackPanel HorizontalAlignment="Right">
                        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,0,0,5">
                            <TextBlock Text="Subtotal:" FontSize="16" Foreground="Gray" Margin="0,0,10,0"/>
                            <TextBlock Text="{Binding SubTotal, StringFormat=C}" FontSize="16" Foreground="Gray"/>
                        </StackPanel>
                        
                        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,0,0,5" Visibility="{Binding DiscountAmount, Converter={StaticResource BooleanToVisibilityConverter}}">
                            <!-- Si DiscountAmount > 0 se muestra -->
                            <TextBlock Text="Promoción Aplicada:" FontSize="14" Foreground="#F59E0B" Margin="0,0,10,0"/>
                            <TextBlock Text="{Binding DiscountAmount, StringFormat='-{0:C}'}" FontSize="14" Foreground="#F59E0B" FontWeight="Bold"/>
                        </StackPanel>

                        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,5,0,0">
                            <TextBlock Text="Total:" FontSize="20" FontWeight="Bold" Margin="0,0,10,0" VerticalAlignment="Center"/>
                            <TextBlock Text="{Binding Total, StringFormat=C}" FontSize="24" FontWeight="Bold" Foreground="#28A745" VerticalAlignment="Center"/>
                        </StackPanel>
                    </StackPanel>
                </Border>`
);

fs.writeFileSync('PosCore/Views/MainWindow.xaml', view);
