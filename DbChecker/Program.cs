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
            Console.WriteLine("Fixing WishlistItems schema...");
            string fixSql = @"
IF OBJECT_ID('WishlistItems', 'U') IS NOT NULL 
BEGIN 
    ALTER TABLE WishlistItems DROP CONSTRAINT IF EXISTS FK_WishlistItems_Equipment_EquipmentId;
    ALTER TABLE WishlistItems DROP CONSTRAINT IF EXISTS FK_WishlistItems_Equipment_EquipmentId1;
    ALTER TABLE WishlistItems DROP CONSTRAINT IF EXISTS FK_WishlistItems_Wishlists_WishlistId;
    DROP TABLE WishlistItems; 
END
IF OBJECT_ID('Wishlists', 'U') IS NOT NULL
BEGIN
    ALTER TABLE Wishlists DROP CONSTRAINT IF EXISTS FK_Wishlists_AspNetUsers_CustomerId;
    DROP TABLE Wishlists;
END

CREATE TABLE WishlistItems (
    Id int NOT NULL IDENTITY,
    CustomerId nvarchar(450) NOT NULL,
    EquipmentId int NOT NULL,
    CreatedAt datetime2 NOT NULL,
    CONSTRAINT PK_WishlistItems PRIMARY KEY (Id),
    CONSTRAINT FK_WishlistItems_AspNetUsers_CustomerId FOREIGN KEY (CustomerId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE,
    CONSTRAINT FK_WishlistItems_Equipment_EquipmentId FOREIGN KEY (EquipmentId) REFERENCES Equipment (Id) ON DELETE CASCADE
);
CREATE INDEX IX_WishlistItems_EquipmentId ON WishlistItems (EquipmentId);
CREATE UNIQUE INDEX IX_WishlistItems_CustomerId_EquipmentId ON WishlistItems (CustomerId, EquipmentId);
";
            using (SqlCommand fixCmd = new SqlCommand(fixSql, conn))
            {
                fixCmd.ExecuteNonQuery();
            }
            Console.WriteLine("Schema fixed successfully.");

            Console.WriteLine("\nReviews Columns:");
            PrintCols("Reviews", conn);

            Console.WriteLine("\nWishlistItems Columns:");
            PrintCols("WishlistItems", conn);
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
