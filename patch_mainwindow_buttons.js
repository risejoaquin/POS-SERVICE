const fs = require('fs');

let view = fs.readFileSync('PosCore/Views/MainWindow.xaml', 'utf8');

view = view.replace(
    '                        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,5,0,0">\n                            <TextBlock Text="Total:" FontSize="20" FontWeight="Bold" Margin="0,0,10,0" VerticalAlignment="Center"/>\n                            <TextBlock Text="{Binding Total, StringFormat=C}" FontSize="24" FontWeight="Bold" Foreground="#28A745" VerticalAlignment="Center"/>\n                        </StackPanel>\n                    </StackPanel>\n                </Border>\n\n                <!-- Botón de Cobro -->\n                <Button Grid.Row="2" Content="Completar Venta (F12)"',
    `                        <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,5,0,0">
                            <TextBlock Text="Total:" FontSize="20" FontWeight="Bold" Margin="0,0,10,0" VerticalAlignment="Center"/>
                            <TextBlock Text="{Binding Total, StringFormat=C}" FontSize="24" FontWeight="Bold" Foreground="#28A745" VerticalAlignment="Center"/>
                        </StackPanel>
                    </StackPanel>
                </Border>

                <!-- Acciones de Orden -->
                <Grid Grid.Row="2" Margin="0,10,0,0">
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="Auto"/>
                    </Grid.RowDefinitions>
                    
                    <Grid Grid.Row="0" Margin="0,0,0,10">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>
                        <Button Grid.Column="0" Content="⏸ Suspender" Command="{Binding SuspendOrderCommand}" Margin="0,0,5,0" Height="40" Background="#F59E0B" Foreground="White" FontWeight="Bold" BorderThickness="0" Cursor="Hand"/>
                        <Button Grid.Column="1" Content="▶ Retomar Orden" Command="{Binding ResumeOrderCommand}" Margin="5,0,0,0" Height="40" Background="#3B82F6" Foreground="White" FontWeight="Bold" BorderThickness="0" Cursor="Hand"/>
                    </Grid>

                    <!-- Botón de Cobro -->
                    <Button Grid.Row="1" Content="Completar Venta (F12)"`
);

fs.writeFileSync('PosCore/Views/MainWindow.xaml', view);
