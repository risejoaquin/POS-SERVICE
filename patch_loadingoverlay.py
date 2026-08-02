with open("PosBuilder/Views/Controls/LoadingOverlay.xaml", "r", encoding="utf-8") as f:
    content = f.read()

new_content = """<UserControl x:Class="PosBuilder.Views.Controls.LoadingOverlay"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008" 
             mc:Ignorable="d" 
             d:DesignHeight="600" d:DesignWidth="800"
             Visibility="Collapsed" Panel.ZIndex="9999">
    <Grid Background="#B3000000">
        <Border Background="White" CornerRadius="8" HorizontalAlignment="Center" VerticalAlignment="Center" Padding="30" MaxWidth="600" MinWidth="400">
            <Border.Effect>
                <DropShadowEffect Color="Black" Opacity="0.2" BlurRadius="15" ShadowDepth="0"/>
            </Border.Effect>
            <Grid>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>
                <ProgressBar Grid.Row="0" x:Name="Spinner" IsIndeterminate="True" Width="60" Height="10" Margin="0,0,0,20"/>
                <TextBlock Grid.Row="1" x:Name="MessageText" Text="Cargando..." FontSize="16" FontWeight="SemiBold" Foreground="#334155" HorizontalAlignment="Center" Margin="0,0,0,15"/>
                
                <Border Grid.Row="2" Background="#1E293B" CornerRadius="6" Padding="10" Margin="0,0,0,0" x:Name="LogContainer" Visibility="Collapsed" MinHeight="150">
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="*"/>
                        </Grid.RowDefinitions>
                        <Grid Grid.Row="0" Margin="0,0,0,5">
                            <TextBlock Text="Logs del Proceso" Foreground="#94A3B8" FontSize="12" FontWeight="Bold"/>
                            <Button Content="Copiar" HorizontalAlignment="Right" Background="#3B82F6" Foreground="White" BorderThickness="0" Padding="10,2" Cursor="Hand" Click="CopyLogs_Click" FontSize="10"/>
                        </Grid>
                        <ScrollViewer Grid.Row="1" MaxHeight="250" x:Name="LogScroller" VerticalScrollBarVisibility="Auto">
                            <TextBox x:Name="LogTextBox" Background="Transparent" Foreground="#10B981" BorderThickness="0" IsReadOnly="True" TextWrapping="Wrap" FontFamily="Consolas" FontSize="11"/>
                        </ScrollViewer>
                    </Grid>
                </Border>
            </Grid>
        </Border>
    </Grid>
</UserControl>"""

with open("PosBuilder/Views/Controls/LoadingOverlay.xaml", "w", encoding="utf-8") as f:
    f.write(new_content)
print("Replaced LoadingOverlay xaml")
