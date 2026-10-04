using System;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connectionString = ""Server=localhost\\SQLEXPRESS;Database=RentKartDb;Trusted_Connection=True;TrustServerCertificate=True;"";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = @""
                ALTER TABLE Reviews ADD BusinessRating int NOT NULL DEFAULT 0;
                ALTER TABLE Reviews ADD EquipmentRating int NOT NULL DEFAULT 0;
                ALTER TABLE Reviews ADD RentalId int NOT NULL DEFAULT 0;
                ALTER TABLE Reviews ADD Status int NOT NULL DEFAULT 0;
                ALTER TABLE Reviews ADD Title nvarchar(max) NOT NULL DEFAULT '';
            "";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
            Console.WriteLine(""Columns added successfully."");
        }
    }
}
