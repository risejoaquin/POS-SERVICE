const fs = require('fs');

let view = fs.readFileSync('PosCore/Views/MainWindow.xaml', 'utf8');

view = view.replace(
    '<DataTrigger Binding="{Binding Notes, Converter={StaticResource IsNullOrEmptyConverter}}" Value="False">',
    '<DataTrigger Binding="{Binding HasNotes}" Value="True">'
);

fs.writeFileSync('PosCore/Views/MainWindow.xaml', view);
