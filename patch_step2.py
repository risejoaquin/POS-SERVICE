with open("PosBuilder/Views/Step2Database.xaml", "r", encoding="utf-8") as f:
    content = f.read()

old_btn = '<Button Content="Test Connection" Command="{Binding TestConnectionCommand}" Padding="10" Background="#3B82F6" Foreground="White" BorderThickness="0" Cursor="Hand"/>'
new_btn = '<Button Content="{Binding TestDbButtonText, FallbackValue=\'Test Connection\'}" Command="{Binding TestConnectionCommand}" Padding="10" Background="#3B82F6" Foreground="White" BorderThickness="0" Cursor="Hand"/>'

if old_btn in content:
    content = content.replace(old_btn, new_btn)
    with open("PosBuilder/Views/Step2Database.xaml", "w", encoding="utf-8") as f:
        f.write(content)
    print("Replaced button in Step 2.")
else:
    print("Could not find button.")
