with open("PosCore/Views/Controls/StatusBarControl.xaml", "r", encoding="utf-8") as f:
    content = f.read()

old_ellipses = '<Ellipse Width="12" Height="12" Fill="{Binding SyncStatusColor}" Margin="0,0,8,0" /><Ellipse Width="12" Height="12" Fill="{Binding SyncStatusColor}" Margin="0,0,8,0" />'
new_ellipses = '<Ellipse Width="12" Height="12" Fill="{Binding SyncStatusColor}" Margin="0,0,8,0" />'

content = content.replace(old_ellipses, new_ellipses)

if '<UserControl.Resources>' not in content:
    old_grid = '<Grid Margin="15,0">'
    new_grid = """    <UserControl.Resources>
        <BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter"/>
    </UserControl.Resources>
    <Grid Margin="15,0">"""
    content = content.replace(old_grid, new_grid)

with open("PosCore/Views/Controls/StatusBarControl.xaml", "w", encoding="utf-8") as f:
    f.write(content)
print("Replaced StatusBarControl.xaml")
