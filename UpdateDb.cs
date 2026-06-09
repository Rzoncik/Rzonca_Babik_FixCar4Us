using System;
using Microsoft.Data.Sqlite;

class UpdateDb
{
    static void Main()
    {
        var connectionString = "Data Source=database.db";
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            try
            {
                var command = connection.CreateCommand();
                command.CommandText = "ALTER TABLE RepairOrders ADD COLUMN AdditionalFee REAL DEFAULT 0;";
                command.ExecuteNonQuery();
                Console.WriteLine("Added AdditionalFee column to RepairOrders table.");
                
                var command2 = connection.CreateCommand();
                command2.CommandText = "ALTER TABLE RepairOrders ADD COLUMN DifficultyDescription TEXT;";
                command2.ExecuteNonQuery();
                Console.WriteLine("Added DifficultyDescription column to RepairOrders table.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (column might already exist): " + ex.Message);
            }
        }
    }
}
