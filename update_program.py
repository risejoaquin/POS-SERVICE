import os

filepath = '/app/applet/PosServer/Program.cs'
with open(filepath, 'r') as f:
    content = f.read()

content = content.replace(
    'options.UseNpgsql(connString));',
    'options.UseNpgsql(connString, o => o.CommandTimeout(120)));'
)

content = content.replace(
'''if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}''',
'''app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => "POS Server is running!");'''
)

with open(filepath, 'w') as f:
    f.write(content)
