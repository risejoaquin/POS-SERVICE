using System;
using BCrypt.Net;

class Program
{
    static void Main()
    {
        string hash = BCrypt.Net.BCrypt.HashPassword("1234");
        Console.WriteLine(BCrypt.Net.BCrypt.Verify("1234", hash));
    }
}
