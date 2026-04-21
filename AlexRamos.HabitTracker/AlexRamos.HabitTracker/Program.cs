using System.Globalization;
using Microsoft.Data.Sqlite;

/// <summary>
/// Habit Logger
/// This programm stores in a database your habits.
/// </summary>
class Program
{
    /// <summary>
    /// Reference to database.
    /// </summary>
    static string connectionString = @"Data Source=habit-Tracker.db";

    static void Main(string[] args)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCommand = connection.CreateCommand();

            tableCommand.CommandText =
                @"CREATE TABLE IF NOT EXISTS drinking_water (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Date TEXT,
                Quantity INTEGER
                )";

            tableCommand.ExecuteNonQuery();

            connection.Close();
        }

        GetUserInput();
    }

    /// <summary>
    /// Display a menu and Get user input. 
    /// </summary>
    static void GetUserInput()
    {
        Console.Clear();
        bool closeApp = false;
        while (closeApp == false)
        {
            Console.WriteLine("\n\nMAIN MENU");
            Console.WriteLine("\nWhat would you like to do?");
            Console.WriteLine("\nType 0 to Close Application.");
            Console.WriteLine("Type 1 to View All Records.");
            Console.WriteLine("Type 2 to Insert All Records.");
            Console.WriteLine("Type 3 to Delete All Records.");
            Console.WriteLine("Type 4 to Update All Records.");
            Console.WriteLine("------------------------------------------------\n");

            string? commandInput = Console.ReadLine();

            switch (commandInput)
            {
                case "0":
                    Console.WriteLine("\nGoodbye!\n");
                    closeApp = true;
                    Environment.Exit(0);
                    break;

                case "1":
                    GetAllRecords();
                    break;

                case "2":
                    Insert();
                    break;

                case "3":
                    Delete();
                    break;

                case "4":
                    Update();
                    break;

                default:
                    Console.WriteLine("\nInvalid Command. Please type a number from 0 to 4.\n");
                    break;
            }
        }
    }

    /// <summary>
    /// Get all the records from the database.
    /// </summary>
    private static void GetAllRecords()
    {
        Console.Clear();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCommand = connection.CreateCommand();
            tableCommand.CommandText =
                $"SELECT * FROM drinking_water";

            List<DrinkingWater> tableData = new();

            SqliteDataReader reader = tableCommand.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    tableData.Add(
                        new DrinkingWater
                        {
                            ID = reader.GetInt32(0),
                            Date = DateTime.ParseExact(reader.GetString(1), "dd-mm-yy", new CultureInfo("en-US")),
                            Quantity = reader.GetInt32(2)
                        }
                    );
                }
            }
            else
            {
                Console.WriteLine("\nNo rows found!\n");
            }

            connection.Close();

            Console.WriteLine("-----------------------------------------------------\n");
            foreach (var drinkingWater in tableData)
            {
                Console.WriteLine($"{drinkingWater.ID} - {drinkingWater.Date:dd-MMM-yyyy} - Quantity: {drinkingWater.Quantity}");
            }
            Console.WriteLine("-----------------------------------------------------\n");
        }
    }

    /// <summary>
    /// Delete a row from the database.
    /// </summary>
    private static void Delete()
    {
        Console.Clear();

        GetAllRecords();

        int recordID = GetNumberInput("\n\nPlease type the ID of the record you want to delete or type 0 to return to Main Menu\n\n");

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var tableCommand = connection.CreateCommand();

            tableCommand.CommandText = $"DELETE from drinking_water WHERE ID = '{recordID}'";

            int rowCount = tableCommand.ExecuteNonQuery();

            if (rowCount == 0)
            {
                Console.WriteLine($"\n\nRecord with ID {recordID} doesn't exist. \n\n");
                
                Console.WriteLine("Press the Enter key to continue.");
                Console.ReadLine();

                Delete();
            }

        }

        Console.WriteLine($"\n\nRecord with ID {recordID} was deleted. \n\n");

        Console.WriteLine("Press the Enter key to continue.");
        Console.ReadLine();

        GetUserInput();
    }

    /// <summary>
    /// Updates a value from the database.
    /// </summary>
    internal static void Update()
    {
        Console.Clear();
        GetAllRecords();

        int recordID = GetNumberInput("\n\nPlease type the ID of the record you want to update or type 0 to return to Main Menu\n\n");

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var checkCommand = connection.CreateCommand();

            checkCommand.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE ID = {recordID})";

            int checkQuery = Convert.ToInt32(checkCommand.ExecuteScalar());

            if (checkQuery == 0)
            {
                Console.WriteLine($"\n\nRecord with ID {recordID} doen't exist.\n\n");
                connection.Close();

                Console.WriteLine("Press the Enter key to continue.");
                Console.ReadLine();

                Update();
            }

            string date = GetDateInput();

            int quantity = GetNumberInput("\n\nPlease insert number of glasses or other measure of your choice (no decimals allowed)\n\n");

            var tableCommand = connection.CreateCommand();

            tableCommand.CommandText = $"UPDATE drinking_water SET Date = '{date}', quantity = {quantity} WHERE ID = {recordID}";

            tableCommand.ExecuteNonQuery();

            connection.Close();
        }

    }

    /// <summary>
    /// Insert new information into the database.
    /// </summary>
    private static void Insert()
    {
        string? date = GetDateInput();

        int quantity = GetNumberInput("\n\nPlease insert number of glasses or other measure of your choice (no decimals allowed)\n\n");

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            var tableCommand = connection.CreateCommand();

            tableCommand.CommandText =
                $"INSERT INTO drinking_water(Date, Quantity) VALUES('{date}', {quantity})";

            tableCommand.ExecuteNonQuery();

            connection.Close();
        }
    }

    /// <summary>
    /// Get user date input (dd-mm-yy)
    /// </summary>
    /// <returns></returns>
    internal static string GetDateInput()
    {
        Console.WriteLine("\n\nPlease insert the date: (Format: dd-mm-yy). Type 0 to return to main menu.\n\n");

        string? dateInput = Console.ReadLine();

        if (dateInput == "0") GetUserInput();

        while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
        {
            Console.WriteLine("\n\nInvalid date, (Format: dd-mm-yy).\n\n");
            dateInput = Console.ReadLine();
        }

        return dateInput;
    }

    /// <summary>
    /// Get user number input.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    internal static int GetNumberInput(string message)
    {
        Console.WriteLine(message);

        string? userInput = Console.ReadLine();

        if (userInput == "0") GetUserInput();

        while (!Int32.TryParse(userInput, out _) || Convert.ToInt32(userInput) < 0)
        {
            Console.WriteLine("\n\nInvalid number. Try again.\n\n");

            userInput = Console.ReadLine();
        }

        int numberInput = Convert.ToInt32(userInput);

        return numberInput;
    }

    /// <summary>
    /// Class that saves drinking water information.
    /// </summary>
    public class DrinkingWater
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }
    }
}
