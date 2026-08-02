import re
with open("PosBuilder/Views/Step7Summary.xaml", "r", encoding="utf-8") as f:
    content = f.read()

old_border = """        <Border Background="#F1F5F9" CornerRadius="8" Padding="15" Margin="0,0,0,20">
            <StackPanel>
                <TextBlock Text="{Binding TenantName, StringFormat='Comercio: {0}'}" FontSize="16" FontWeight="SemiBold"/>
                <TextBlock Text="{Binding Environment, StringFormat='Entorno: {0}'}" FontSize="14"/>
                <TextBlock Text="{Binding DbType, StringFormat='Base de Datos: {0}'}" FontSize="14"/>
            </StackPanel>
        </Border>"""

new_border = """        <Border Background="#F1F5F9" CornerRadius="8" Padding="20" Margin="0,0,0,20">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="*"/>
                </Grid.ColumnDefinitions>
                <StackPanel Grid.Column="0">
                    <TextBlock Text="Información General" FontWeight="Bold" Margin="0,0,0,10" Foreground="#3B82F6"/>
                    <TextBlock Text="{Binding TenantName, StringFormat='Comercio: {0}'}" FontSize="14" Margin="0,0,0,5"/>
                    <TextBlock Text="{Binding Environment, StringFormat='Entorno: {0}'}" FontSize="14" Margin="0,0,0,5"/>
                    <TextBlock Text="{Binding ApiUrl, StringFormat='API URL: {0}'}" FontSize="14" Margin="0,0,0,5" TextTrimming="CharacterEllipsis" MaxWidth="200" HorizontalAlignment="Left" ToolTip="{Binding ApiUrl}"/>
                    <TextBlock Text="{Binding BrandingName, StringFormat='Nombre POS: {0}'}" FontSize="14" Margin="0,0,0,5"/>
                </StackPanel>
                <StackPanel Grid.Column="1">
                    <TextBlock Text="Configuración Técnica" FontWeight="Bold" Margin="0,0,0,10" Foreground="#3B82F6"/>
                    <TextBlock Text="{Binding DbType, StringFormat='Base de Datos: {0}'}" FontSize="14" Margin="0,0,0,5"/>
                    <TextBlock Text="Usuarios: Administrador, Empleado" FontSize="14" Margin="0,0,0,5"/>
                    <TextBlock Text="Seguridad: JWT Configurado" FontSize="14" Margin="0,0,0,5"/>
                </StackPanel>
            </Grid>
        </Border>
"""

content = content.replace(old_border, new_border)
with open("PosBuilder/Views/Step7Summary.xaml", "w", encoding="utf-8") as f:
    f.write(content)
print("Replaced Step7Summary xaml")
