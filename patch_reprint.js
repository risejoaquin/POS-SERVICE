const fs = require('fs');
let content = fs.readFileSync('PosCore/Views/ReturnsWindow.xaml', 'utf8');

content = content.replace(
    '                                        Padding="10,6" Background="#EF4444" Foreground="White" BorderThickness="0" Cursor="Hand" HorizontalAlignment="Left"/>\n                                <DataTemplate.Triggers>',
    '                                        Padding="10,6" Background="#EF4444" Foreground="White" BorderThickness="0" Cursor="Hand" HorizontalAlignment="Left" Margin="0,0,5,0"/>\n                                <Button x:Name="BtnReprint" Content="Reimprimir"\n                                         Command="{Binding DataContext.ReprintOrderCommand, RelativeSource={RelativeSource AncestorType=Window}}"\n                                         CommandParameter="{Binding}"\n                                        Padding="10,6" Background="#3B82F6" Foreground="White" BorderThickness="0" Cursor="Hand" HorizontalAlignment="Left"/>\n                                <DataTemplate.Triggers>'
);

fs.writeFileSync('PosCore/Views/ReturnsWindow.xaml', content);

let vmContent = fs.readFileSync('PosCore/ViewModels/ReturnsViewModel.cs', 'utf8');
vmContent = vmContent.replace(
    '        LoadOrdersCommand.Execute(null);\n    }',
    '        LoadOrdersCommand.Execute(null);\n    }\n\n    [RelayCommand]\n    private void ReprintOrder(Order order)\n    {\n        if (order == null) return;\n        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))\n        {\n            bool success = _ticketPrinterService.PrintTicket(order);\n            if (success)\n                MessageBox.Show("Ticket enviado a la impresora.", "Reimpresión", MessageBoxButton.OK, MessageBoxImage.Information);\n            else\n                MessageBox.Show("Error al imprimir el ticket. Revise la impresora.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);\n        }\n    }'
);

fs.writeFileSync('PosCore/ViewModels/ReturnsViewModel.cs', vmContent);
