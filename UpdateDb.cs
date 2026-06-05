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
                command.CommandText = "ALTER TABLE Customers ADD COLUMN PasswordHash TEXT;";
                command.ExecuteNonQuery();
                Console.WriteLine("Added PasswordHash column to Customers table.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error (column might already exist): " + ex.Message);
            }
        }
    }
}
