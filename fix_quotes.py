with open("PosBuilder/MainWindow.xaml.cs", "r", encoding="utf-8") as f:
    content = f.read()

content = content.replace('Arguments = $"run --project "{serverPath}""', 'Arguments = $"run --project \\"{serverPath}\\""')
content = content.replace('Arguments = $"run --project "{corePath}""', 'Arguments = $"run --project \\"{corePath}\\""')

with open("PosBuilder/MainWindow.xaml.cs", "w", encoding="utf-8") as f:
    f.write(content)
