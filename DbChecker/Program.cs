using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connectionString = @"Server=localhost\SQLEXPRESS;Database=RentKartDb;Trusted_Connection=True;";
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string tableName = reader.GetString(0);
                    Console.WriteLine("Table: " + tableName);
                }
            }

            Console.WriteLine("\nCounts:");
            string[] tables = { "AspNetUsers", "Businesses", "Equipment", "Notifications", "Reviews" };
            foreach(var t in tables)
            {
                try {
                    SqlCommand countCmd = new SqlCommand($"SELECT COUNT(*) FROM [{t}]", conn);
                    int count = (int)countCmd.ExecuteScalar();
                    Console.WriteLine($"{t}: {count} rows");
                }
                catch(Exception ex) {
                    Console.WriteLine($"{t}: Error - {ex.Message}");
                }
            }

            Console.WriteLine("\nNotifications Columns:");
            PrintCols("Notifications", conn);
            
            Console.WriteLine("\nBusinesses Columns:");
            PrintCols("Businesses", conn);

            Console.WriteLine("\nEquipment Columns:");
            PrintCols("Equipment", conn);
        }
    }

    static void PrintCols(string tableName, SqlConnection conn)
    {
        SqlCommand colsCmd = new SqlCommand($"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'", conn);
        using (SqlDataReader colReader = colsCmd.ExecuteReader())
        {
            while (colReader.Read())
            {
                Console.WriteLine(" - " + colReader.GetString(0));
            }
        }
    }
}
