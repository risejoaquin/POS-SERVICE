const fs = require('fs');
let content = fs.readFileSync('PosCore/Views/MainWindow.xaml', 'utf8');

// Replace Title to just be the company name, offline status will be handled in the UI
content = content.replace(
    'Title="{Binding Settings.WhiteLabel.CompanyName, StringFormat=\'{}POS - {0} (Modo Offline)\'}"',
    'Title="{Binding Settings.WhiteLabel.CompanyName, StringFormat=\'{}POS - {0}\'}"'
);

// Add Top Header for Sync Status
const headerXaml = `    <Grid Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Top Header for Network & Sync Status -->
        <Border Grid.Row="0" Grid.ColumnSpan="2" Background="{Binding PrimaryColorBrush}" CornerRadius="4" Padding="10,5" Margin="0,0,0,10">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="Auto"/>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>
                <TextBlock Text="{Binding Settings.WhiteLabel.CompanyName, StringFormat='{}☕ {0}'}" FontSize="16" FontWeight="Bold" Foreground="White" VerticalAlignment="Center"/>
                
                <StackPanel Grid.Column="2" Orientation="Horizontal" VerticalAlignment="Center">
                    <Ellipse Width="10" Height="10" Fill="{Binding SyncStatusColor}" Margin="0,0,5,0" />
                    <TextBlock Text="{Binding SyncStatusMessage}" Foreground="White" FontSize="14" FontWeight="SemiBold"/>
                </StackPanel>
            </Grid>
        </Border>

        <Grid Grid.Row="1">`;

content = content.replace(
    '    <Grid Margin="10">\n        <Grid.RowDefinitions>\n            <RowDefinition Height="*"/>\n            <RowDefinition Height="Auto"/>\n        </Grid.RowDefinitions>',
    headerXaml
);

// Adjust row indices inside the inner Grid
content = content.replace(
    '        <Grid.ColumnDefinitions>\n            <ColumnDefinition Width="2*"/>\n            <ColumnDefinition Width="1*"/>\n        </Grid.ColumnDefinitions>',
    '            <Grid.ColumnDefinitions>\n                <ColumnDefinition Width="2*"/>\n                <ColumnDefinition Width="1*"/>\n            </Grid.ColumnDefinitions>'
);

// Fix inner Grid.Row for GroupBoxes and modules
content = content.replace(
    '        <!-- Módulos Opcionales -->\n        <WrapPanel Grid.Row="1"',
    '    </Grid>\n        <!-- Módulos Opcionales -->\n        <WrapPanel Grid.Row="2"'
);

content = content.replace(
    '        <!-- Toast Notification Overlay -->\n        <Border Grid.Row="0" Grid.RowSpan="2"',
    '        <!-- Toast Notification Overlay -->\n        <Border Grid.Row="0" Grid.RowSpan="3"'
);

fs.writeFileSync('PosCore/Views/MainWindow.xaml', content);
