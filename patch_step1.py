import re

with open("PosBuilder/Views/Step1Environment.xaml", "r", encoding="utf-8") as f:
    content = f.read()

old_url_field = """        <TextBlock Text="API URL" FontWeight="SemiBold" Margin="0,0,0,5"/>
        <TextBox ToolTip="URL base donde se desplegará o consumirá la API." Text="{Binding ApiUrl, UpdateSourceTrigger=PropertyChanged}" Padding="8" Margin="0,0,0,15" FontSize="14"/>"""

new_url_field = """        <TextBlock Text="API URL" FontWeight="SemiBold" Margin="0,0,0,5"/>
        <Grid Margin="0,0,0,15">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <TextBox Grid.Column="0" ToolTip="URL base donde se desplegará o consumirá la API." Text="{Binding ApiUrl, UpdateSourceTrigger=PropertyChanged}" Padding="8" FontSize="14"/>
            <Button Grid.Column="1" Command="{Binding TestApiCommand}" Content="{Binding TestApiButtonText, FallbackValue='Probar'}" Margin="10,0,0,0" Padding="15,0" Background="#3B82F6" Foreground="White" BorderThickness="0" Cursor="Hand" />
        </Grid>"""

if old_url_field in content:
    content = content.replace(old_url_field, new_url_field)
    with open("PosBuilder/Views/Step1Environment.xaml", "w", encoding="utf-8") as f:
        f.write(content)
    print("Replaced URL field.")
else:
    print("Could not find URL field.")
