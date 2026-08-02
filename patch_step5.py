import re
with open("PosBuilder/Views/Step5Users.xaml", "r", encoding="utf-8") as f:
    content = f.read()

old_admin_pass = """        <TextBlock Text="Contraseña Admin" FontWeight="SemiBold" Margin="0,0,0,5"/>
        <TextBox ToolTip="Contraseña inicial para el administrador." Text="{Binding AdminPassword, UpdateSourceTrigger=PropertyChanged}" Padding="8" Margin="0,0,0,25" />"""

new_admin_pass = """        <TextBlock Text="Contraseña Admin" FontWeight="SemiBold" Margin="0,0,0,5"/>
        <PasswordBox x:Name="AdminPassBox" PasswordChanged="AdminPassBox_PasswordChanged" Padding="8" Margin="0,0,0,5" />
        <TextBlock x:Name="AdminPassWarning" Text="Mínimo 6 caracteres recomendados" FontSize="11" Foreground="Orange" Visibility="Collapsed" Margin="0,0,0,20"/>"""

old_emp_pass = """        <TextBlock Text="Contraseña Empleado" FontWeight="SemiBold" Margin="0,0,0,5"/>
        <TextBox ToolTip="Contraseña para el empleado inicial." Text="{Binding EmployeePassword, UpdateSourceTrigger=PropertyChanged}" Padding="8" Margin="0,0,0,15" />"""

new_emp_pass = """        <TextBlock Text="Contraseña Empleado" FontWeight="SemiBold" Margin="0,0,0,5"/>
        <PasswordBox x:Name="EmpPassBox" PasswordChanged="EmpPassBox_PasswordChanged" Padding="8" Margin="0,0,0,5" />
        <TextBlock x:Name="EmpPassWarning" Text="Mínimo 6 caracteres recomendados" FontSize="11" Foreground="Orange" Visibility="Collapsed" Margin="0,0,0,10"/>"""

content = content.replace(old_admin_pass, new_admin_pass)
content = content.replace(old_emp_pass, new_emp_pass)

with open("PosBuilder/Views/Step5Users.xaml", "w", encoding="utf-8") as f:
    f.write(content)
print("Replaced Step5Users xaml")
