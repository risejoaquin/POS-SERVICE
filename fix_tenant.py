with open("PosServer/Middlewares/TenantMiddleware.cs", "r") as f:
    content = f.read()

old_code = """        bool isExemptRoute = path.Contains("/api/auth/login") || path.Contains("/api/license/validate") || path.Contains("/api/license/generate");"""
new_code = """        bool isExemptRoute = path == "/" || path.Contains("/swagger") || path.Contains("/api/auth/login") || path.Contains("/api/license/validate") || path.Contains("/api/license/generate");"""

if old_code in content:
    content = content.replace(old_code, new_code)
    with open("PosServer/Middlewares/TenantMiddleware.cs", "w") as f:
        f.write(content)
    print("Replaced middleware")
else:
    print("Not found")
