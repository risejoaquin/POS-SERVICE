using System;
using System.Net;

class Program
{
    static void Main()
    {
        var connString = "postgresql://postgres.aklyqyrfhkimxxgbdhqy:password123@aws-1-us-east-2.pooler.supabase.com:5432/postgres";
        var uri = new Uri(connString);
        var userInfo = uri.UserInfo.Split(':', 2); 
        var username = WebUtility.UrlDecode(userInfo[0]);
        var password = userInfo.Length > 1 ? WebUtility.UrlDecode(userInfo[1]) : "";
        connString = $"Host={uri.Host};Port={(uri.IsDefaultPort ? 5432 : uri.Port)};Database={uri.LocalPath.TrimStart('/')};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=True";
        Console.WriteLine(connString);
    }
}
