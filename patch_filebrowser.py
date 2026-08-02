import re
with open("PosBuilder/Views/Controls/FileBrowserControl.xaml", "r", encoding="utf-8") as f:
    content = f.read()

old_preview = """                <StackPanel x:Name="PreviewState" Visibility="Collapsed" HorizontalAlignment="Center" VerticalAlignment="Center" IsHitTestVisible="False">
                    <Image x:Name="Thumbnail" Width="100" Height="100" Stretch="Uniform" Margin="0,0,0,10"/>
                    <TextBlock x:Name="FileNameText" Foreground="#334155" FontSize="12" TextWrapping="Wrap" MaxWidth="250" HorizontalAlignment="Center"/>
                </StackPanel>"""

new_preview = """                <StackPanel x:Name="PreviewState" Visibility="Collapsed" HorizontalAlignment="Center" VerticalAlignment="Center" IsHitTestVisible="False">
                    <Border CornerRadius="8" Background="#E2E8F0" Padding="5" Margin="0,0,0,10">
                        <Image x:Name="Thumbnail" Width="100" Height="100" Stretch="Uniform" />
                    </Border>
                    <TextBlock x:Name="FileNameText" Foreground="#334155" FontSize="12" TextTrimming="CharacterEllipsis" MaxWidth="200" HorizontalAlignment="Center" ToolTip="{Binding Text, RelativeSource={RelativeSource Self}}"/>
                    <TextBlock Text="Clic para cambiar (PNG recomendado)" Foreground="#64748B" FontSize="10" HorizontalAlignment="Center" Margin="0,5,0,0"/>
                </StackPanel>"""

if old_preview in content:
    content = content.replace(old_preview, new_preview)
    with open("PosBuilder/Views/Controls/FileBrowserControl.xaml", "w", encoding="utf-8") as f:
        f.write(content)
    print("Replaced FileBrowserControl xaml")
else:
    print("Could not find old_preview")
