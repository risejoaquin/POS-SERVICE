using System;
using Npgsql;

class Program
{
    static void Main()
    {
        try {
            var builder = new NpgsqlConnectionStringBuilder("Host=localhost;Database=posdb;Username=postgres;Password=postgres");
            Console.WriteLine("Success 1");
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
        
        try {
            var builder = new NpgsqlConnectionStringBuilder("Data Source=posdb.sqlite");
            Console.WriteLine("Success 2");
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
