const fs = require('fs');
let content = fs.readFileSync('PosCore/Views/MainWindow.xaml', 'utf8');

content = content.replace(
    '        <!-- Hardware Error Banner -->\n        <Border Grid.Row="0" Grid.ColumnSpan="2" Background="#DC2626" CornerRadius="4" Padding="10,5" Margin="0,0,0,10"\n                Visibility="{Binding IsHardwareError, Converter={StaticResource BooleanToVisibilityConverter}}">',
    '        <!-- Header Group -->\n        <StackPanel Grid.Row="0" Grid.ColumnSpan="2">\n        <!-- Hardware Error Banner -->\n        <Border Background="#DC2626" CornerRadius="4" Padding="10,5" Margin="0,0,0,10"\n                Visibility="{Binding IsHardwareError, Converter={StaticResource BooleanToVisibilityConverter}}">'
);

content = content.replace(
    '        <!-- Top Header for Network & Sync Status -->\n        <Border Grid.Row="0" Grid.ColumnSpan="2" Background="{Binding PrimaryColorBrush}" CornerRadius="4" Padding="10,5" Margin="0,0,0,10">',
    '        <!-- Top Header for Network & Sync Status -->\n        <Border Background="{Binding PrimaryColorBrush}" CornerRadius="4" Padding="10,5" Margin="0,0,0,10">'
);

content = content.replace(
    '                </StackPanel>\n            </Grid>\n        </Border>\n\n        <Grid Grid.Row="1">',
    '                </StackPanel>\n            </Grid>\n        </Border>\n        </StackPanel>\n\n        <Grid Grid.Row="1">'
);

fs.writeFileSync('PosCore/Views/MainWindow.xaml', content);
