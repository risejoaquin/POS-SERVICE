const fs = require('fs');

let view = fs.readFileSync('PosCore/Views/MainWindow.xaml', 'utf8');

let newCartItem = `                            <Grid Margin="0,5">
                                <Grid.RowDefinitions>
                                    <RowDefinition Height="Auto"/>
                                    <RowDefinition Height="Auto"/>
                                </Grid.RowDefinitions>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="2*"/>
                                    <ColumnDefinition Width="Auto"/>
                                    <ColumnDefinition Width="1*"/>
                                    <ColumnDefinition Width="Auto"/>
                                    <ColumnDefinition Width="Auto"/>
                                </Grid.ColumnDefinitions>
                                
                                <TextBlock Text="{Binding Product.Name}" Grid.Row="0" Grid.Column="0" VerticalAlignment="Center" TextTrimming="CharacterEllipsis" Margin="0,0,5,0" FontWeight="SemiBold"/>
                                
                                <StackPanel Grid.Row="0" Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Center" Margin="5,0">
                                    <Button Content="-" Width="22" Height="22"
                                             Command="{Binding DataContext.DecreaseQuantityCommand, RelativeSource={RelativeSource AncestorType=Window}}"
                                             CommandParameter="{Binding}"
                                             Background="#E5E7EB" BorderThickness="0" Cursor="Hand" FontWeight="Bold"/>
                                    <TextBlock Text="{Binding Quantity}" Width="26" TextAlignment="Center" VerticalAlignment="Center"/>
                                    <Button Content="+" Width="22" Height="22"
                                             Command="{Binding DataContext.IncreaseQuantityCommand, RelativeSource={RelativeSource AncestorType=Window}}"
                                             CommandParameter="{Binding}"
                                             Background="#E5E7EB" BorderThickness="0" Cursor="Hand" FontWeight="Bold"/>
                                </StackPanel>
                                
                                <TextBlock Text="{Binding SubTotal, StringFormat=C}" Grid.Row="0" Grid.Column="2" VerticalAlignment="Center" HorizontalAlignment="Right" FontWeight="Bold" Margin="0,0,10,0"/>
                                
                                <Button Grid.Row="0" Grid.Column="3" Content="✎" Width="26" Height="26" Margin="0,0,5,0"
                                         Command="{Binding DataContext.ModifyItemCommand, RelativeSource={RelativeSource AncestorType=Window}}"
                                         CommandParameter="{Binding}"
                                         Background="#3B82F6" Foreground="White" BorderThickness="0" Cursor="Hand" ToolTip="Modificar (Notas, Descuento)"/>

                                <Button Grid.Row="0" Grid.Column="4" Content="✕" Width="26" Height="26"
                                         Command="{Binding DataContext.RemoveFromCartCommand, RelativeSource={RelativeSource AncestorType=Window}}"
                                         CommandParameter="{Binding}"
                                         Background="#EF4444" Foreground="White" BorderThickness="0" Cursor="Hand" FontWeight="Bold" ToolTip="Quitar"/>

                                <!-- Detalles adicionales (Notas, Descuentos) -->
                                <StackPanel Grid.Row="1" Grid.Column="0" Grid.ColumnSpan="3" Margin="10,2,0,0">
                                    <TextBlock Text="{Binding Notes, StringFormat='{}Nota: {0}'}" FontSize="12" Foreground="Gray" Visibility="{Binding Notes, Converter={StaticResource StringVisibilityConverter}}"/>
                                    <TextBlock Text="{Binding Discount, StringFormat='{}Descuento: -{0:C}'}" FontSize="12" Foreground="#F59E0B" Visibility="{Binding Discount, Converter={StaticResource NumericVisibilityConverter}}"/>
                                </StackPanel>
                            </Grid>`;

view = view.replace(
    /<DataTemplate>[\s\S]*?<\/DataTemplate>/,
    `<DataTemplate>\n${newCartItem}\n                        </DataTemplate>`
);

// We need custom converters. For now, let's just make a simple DataTrigger or write converter classes, but we can't easily add classes to XAML resources from here without modifying App.xaml.
// Actually, let's use DataTriggers to handle visibility to avoid writing Converters.
