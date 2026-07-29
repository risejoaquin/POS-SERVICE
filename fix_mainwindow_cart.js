const fs = require('fs');

let view = fs.readFileSync('PosCore/Views/MainWindow.xaml', 'utf8');

view = view.replace(
    '<!-- Detalles adicionales (Notas, Descuentos) -->\n                                <StackPanel Grid.Row="1" Grid.Column="0" Grid.ColumnSpan="3" Margin="10,2,0,0">\n                                    <TextBlock Text="{Binding Notes, StringFormat=\'{}Nota: {0}\'}" FontSize="12" Foreground="Gray" Visibility="{Binding Notes, Converter={StaticResource StringVisibilityConverter}}"/>\n                                    <TextBlock Text="{Binding Discount, StringFormat=\'{}Descuento: -{0:C}\'}" FontSize="12" Foreground="#F59E0B" Visibility="{Binding Discount, Converter={StaticResource NumericVisibilityConverter}}"/>\n                                </StackPanel>',
    `<!-- Detalles adicionales (Notas, Descuentos) -->
                                <StackPanel Grid.Row="1" Grid.Column="0" Grid.ColumnSpan="3" Margin="10,2,0,0">
                                    <TextBlock x:Name="NotesText" Text="{Binding Notes, StringFormat='{}Nota: {0}'}" FontSize="12" Foreground="Gray" Visibility="Collapsed"/>
                                    <TextBlock x:Name="DiscountText" Text="{Binding Discount, StringFormat='{}Descuento: -{0:C}'}" FontSize="12" Foreground="#F59E0B" Visibility="Collapsed"/>
                                </StackPanel>
                            </Grid>
                            <DataTemplate.Triggers>
                                <DataTrigger Binding="{Binding Notes, Converter={StaticResource IsNullOrEmptyConverter}}" Value="False">
                                    <Setter TargetName="NotesText" Property="Visibility" Value="Visible"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding HasDiscount}" Value="True">
                                    <Setter TargetName="DiscountText" Property="Visibility" Value="Visible"/>
                                </DataTrigger>
                            </DataTemplate.Triggers>`
);

fs.writeFileSync('PosCore/Views/MainWindow.xaml', view);
