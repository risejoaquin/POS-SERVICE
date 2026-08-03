with open("PosCore/App.xaml", "r", encoding="utf-8") as f:
    content = f.read()

old_resources = """    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>"""

new_resources = """    <Application.Resources>
        <ResourceDictionary>
            <BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter" />
            <ResourceDictionary.MergedDictionaries>"""

content = content.replace(old_resources, new_resources)

with open("PosCore/App.xaml", "w", encoding="utf-8") as f:
    f.write(content)
print("Replaced App.xaml")
