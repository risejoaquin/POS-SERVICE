import re
with open("PosBuilder/Views/Step3Security.xaml", "r", encoding="utf-8") as f:
    content = f.read()

old_jwt = """        <PasswordBox x:Name="JwtSecretBox" PasswordChanged="JwtSecretBox_PasswordChanged" Padding="8" Margin="0,0,0,5" />"""

new_jwt = """        <Grid Margin="0,0,0,5">
            <PasswordBox x:Name="JwtSecretBox" PasswordChanged="JwtSecretBox_PasswordChanged" Padding="8,8,30,8" />
            <TextBox x:Name="JwtSecretVisibleBox" Text="{Binding JwtSecret, UpdateSourceTrigger=PropertyChanged}" Visibility="Collapsed" Padding="8,8,30,8" TextChanged="JwtSecretVisibleBox_TextChanged" />
            <ToggleButton x:Name="ToggleJwtVisibility" Content="👁" Width="25" Height="25" HorizontalAlignment="Right" Margin="0,0,5,0" Background="Transparent" BorderThickness="0" Click="ToggleJwtVisibility_Click" />
        </Grid>"""

if old_jwt in content:
    content = content.replace(old_jwt, new_jwt)
    with open("PosBuilder/Views/Step3Security.xaml", "w", encoding="utf-8") as f:
        f.write(content)
    print("Replaced Step3Security xaml")
else:
    print("Could not find old_jwt")
