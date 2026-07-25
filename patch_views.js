const fs = require('fs');

// Patch ReturnsWindow.xaml
let returnsXml = fs.readFileSync('PosCore/Views/ReturnsWindow.xaml', 'utf8');
returnsXml = returnsXml.replace(
    `<StackPanel Orientation="Horizontal" HorizontalAlignment="SpaceBetween">
                <TextBlock Text="Devoluciones de Órdenes" FontSize="24" FontWeight="Bold" Foreground="#111827"/>
                <Button Command="{Binding LoadOrdersCommand}" Content="Actualizar Lista" Padding="15,8" Background="#3B82F6" Foreground="White" BorderThickness="0" Cursor="Hand"/>
            </StackPanel>`,
    `<Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="Auto" />
                </Grid.ColumnDefinitions>
                <TextBlock Text="Devoluciones de Órdenes" FontSize="24" FontWeight="Bold" Foreground="#111827" VerticalAlignment="Center"/>
                <Button Grid.Column="1" Command="{Binding LoadOrdersCommand}" Content="Actualizar Lista" Padding="15,8" Background="#3B82F6" Foreground="White" BorderThickness="0" Cursor="Hand"/>
            </Grid>`
);
fs.writeFileSync('PosCore/Views/ReturnsWindow.xaml', returnsXml);

// Patch ShiftWindow.xaml
let shiftXml = fs.readFileSync('PosCore/Views/ShiftWindow.xaml', 'utf8');
shiftXml = shiftXml.replace(
    `<StackPanel Orientation="Horizontal" HorizontalAlignment="SpaceBetween">
                            <TextBlock Text="Efectivo Esperado:" FontSize="16" FontWeight="SemiBold" Foreground="#374151" VerticalAlignment="Center"/>
                            <TextBlock Text="{Binding CalculatedExpectedCash, StringFormat='C'}" FontSize="24" FontWeight="Bold" Foreground="#2563EB"/>
                        </StackPanel>`,
    `<Grid>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*" />
                                <ColumnDefinition Width="Auto" />
                            </Grid.ColumnDefinitions>
                            <TextBlock Text="Efectivo Esperado:" FontSize="16" FontWeight="SemiBold" Foreground="#374151" VerticalAlignment="Center"/>
                            <TextBlock Grid.Column="1" Text="{Binding CalculatedExpectedCash, StringFormat='C'}" FontSize="24" FontWeight="Bold" Foreground="#2563EB" VerticalAlignment="Center"/>
                        </Grid>`
);
fs.writeFileSync('PosCore/Views/ShiftWindow.xaml', shiftXml);

