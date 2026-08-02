with open("PosBuilder/Views/Controls/LoadingOverlay.xaml.cs", "r", encoding="utf-8") as f:
    content = f.read()

content = "using System;\\n" + content

with open("PosBuilder/Views/Controls/LoadingOverlay.xaml.cs", "w", encoding="utf-8") as f:
    f.write(content)
