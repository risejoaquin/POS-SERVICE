const fs = require('fs');
let content = fs.readFileSync('PosCore/Views/MainWindow.xaml', 'utf8');

content = content.replace(
    '        <!-- Top Header for Network & Sync Status -->',
    `        <!-- Hardware Error Banner -->
        <Border Grid.Row="0" Grid.ColumnSpan="2" Background="#DC2626" CornerRadius="4" Padding="10,5" Margin="0,0,0,10"
                Visibility="{Binding IsHardwareError, Converter={StaticResource BooleanToVisibilityConverter}}">
            <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
                <TextBlock Text="⚠ " FontSize="16" FontWeight="Bold" Foreground="White" VerticalAlignment="Center"/>
                <TextBlock Text="{Binding HardwareErrorMessage}" FontSize="14" FontWeight="SemiBold" Foreground="White" VerticalAlignment="Center"/>
                <Button Content="Ignorar / Cerrar" Margin="15,0,0,0" Padding="10,2" Background="White" Foreground="#DC2626" FontWeight="Bold" BorderThickness="0" Cursor="Hand" Click="DismissHardwareError_Click"/>
            </StackPanel>
        </Border>

        <!-- Top Header for Network & Sync Status -->`
);

// We need to shift Row indexing again because we added a banner at row 0? 
// No, both can be at Row 0, but it's better to put them in a StackPanel or define more rows.
// Let's replace the whole top part cleanly.
