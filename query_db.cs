using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connStr = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=RentKartDb;Integrated Security=True;Encrypt=False;";
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();
            Console.WriteLine("--- EFMigrationsHistory ---");
            using (SqlCommand cmd = new SqlCommand("SELECT MigrationId FROM __EFMigrationsHistory", conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(reader["MigrationId"]);
                    }
                }
            }
            Console.WriteLine("--- Tables and Columns ---");
            using (SqlCommand cmd = new SqlCommand("SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS ORDER BY TABLE_NAME", conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["TABLE_NAME"]}.{reader["COLUMN_NAME"]}");
                    }
                }
            }
        }
    }
}
