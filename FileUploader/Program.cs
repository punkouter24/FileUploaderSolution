using System;
using System.Data.SqlClient;
using System.IO;

namespace FileUploader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: FileUploader <file-path>");
                return;
            }

            string filePath = args[0];
            string fileName = Path.GetFileName(filePath);
            byte[] fileContent = File.ReadAllBytes(filePath);

            string connectionString = "Data Source=(localdb)\\ProjectModels;Initial Catalog=FileDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Connection to database succeeded.");

                    string query = "INSERT INTO Files (FileName, FileContent) VALUES (@FileName, @FileContent)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FileName", fileName);
                        command.Parameters.AddWithValue("@FileContent", fileContent);

                        command.ExecuteNonQuery();
                    }
                }

                Console.WriteLine("File uploaded successfully.");
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL Exception: {sqlEx.Message}");
                Console.WriteLine($"Error Number: {sqlEx.Number}");
                Console.WriteLine($"State: {sqlEx.State}");
                Console.WriteLine($"Class: {sqlEx.Class}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
