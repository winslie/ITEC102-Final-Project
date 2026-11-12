using System;
using static System.Console;

class Program
{
    // Accounts list stored in arrays
    static string[] platforms = new string[100];
    static string[] usernames = new string[100];
    static string[] passwords = new string[100];
    static int total = 0;

    static Random rng = new Random();

    // START PasswordManager
    static void Main()
    {
        string choice;
        bool isRunning = true;

        if (System.IO.File.Exists("accounts.txt"))
        {
            string[] lines = System.IO.File.ReadAllLines("accounts.txt");
            int i = 0;
            while (i + 2 < lines.Length && total < 100)
            {
                platforms[total] = lines[i];
                usernames[total] = lines[i + 1];
                passwords[total] = lines[i + 2];
                total++;
                i += 3;
            }
        }
        else
        {
            total = 0;
        }

        while (isRunning)
        {
            WriteLine("===== PASSWORD MANAGER =====");
            WriteLine("1. Add Account");
            WriteLine("2. View All Accounts");
            WriteLine("3. Search Account");
            WriteLine("4. Update Account");
            WriteLine("5. Delete Account");
            WriteLine("6. Generate Random Password");
            WriteLine("7. Exit");
            Write("Enter choice: ");
            choice = ReadLine();

            if (choice == "1")
            {
                AddAccount();
            }
            else if (choice == "2")
            {
                ViewAccounts();
            }
            else if (choice == "3")
            {
                SearchAccount();
            }
            else if (choice == "4")
            {
                UpdateAccount();
            }
            else if (choice == "5")
            {
                DeleteAccount();
            }
            else if (choice == "6")
            {
                string password = GeneratePassword();
                WriteLine("Generated password: " + password);
            }
            else if (choice == "7")
            {
                string[] lines = new string[total * 3];
                for (int i = 0; i < total; i++)
                {
                    lines[i * 3] = platforms[i];
                    lines[i * 3 + 1] = usernames[i];
                    lines[i * 3 + 2] = passwords[i];
                }
                System.IO.File.WriteAllLines("accounts.txt", lines);
                WriteLine("Goodbye!");
                isRunning = false;
            }
            else
            {
                WriteLine("Invalid choice. Please enter a number from 1 to 7.");
            }
            WriteLine();
        }
    }
    // END PasswordManager

    // START AddAccount
    static void AddAccount()
    {
        if (total == 100)
        {
            WriteLine("Account list is full.");
            return;
        }

        Write("Enter platform: ");
        string platform = ReadLine();
        while (platform == "")
        {
            WriteLine("Platform name cannot be empty.");
            Write("Enter platform: ");
            platform = ReadLine();
        }

        Write("Enter username: ");
        string username = ReadLine();
        while (username == "")
        {
            WriteLine("Username cannot be empty.");
            Write("Enter username: ");
            username = ReadLine();
        }

        WriteLine("1. Enter my own password 2. Generate a random password");
        Write("Enter option: ");
        string option = ReadLine();
        while (option != "1" && option != "2")
        {
            WriteLine("Invalid option. Enter 1 or 2.");
            Write("Enter option: ");
            option = ReadLine();
        }

        string password;
        if (option == "2")
        {
            password = GeneratePassword();
            WriteLine("Generated password: " + password);
        }
        else
        {
            Write("Enter password: ");
            password = ReadLine();
            while (password == "")
            {
                WriteLine("Password cannot be empty.");
                Write("Enter password: ");
                password = ReadLine();
            }
        }

        platforms[total] = platform;
        usernames[total] = username;
        passwords[total] = password;
        total++;
        WriteLine("Account saved successfully.");
    }
    // END AddAccount

    // START ViewAccounts
    static void ViewAccounts()
    {
        if (total == 0)
        {
            WriteLine("No saved accounts.");
        }
        else
        {
            int number = 1;
            while (number <= total)
            {
                WriteLine(number + ". Platform: " + platforms[number - 1] +
                                  " | Username: " + usernames[number - 1] +
                                  " | Password: " + passwords[number - 1]);
                number++;
            }
        }
    }
    // END ViewAccounts

    // START SearchAccount
    static void SearchAccount()
    {
        Write("Enter keyword: ");
        string keyword = ReadLine();
        bool found = false;
        int number = 1;

        while (number <= total)
        {
            if (platforms[number - 1].ToLower().Contains(keyword.ToLower()))
            {
                WriteLine("Platform: " + platforms[number - 1] +
                                  " | Username: " + usernames[number - 1] +
                                  " | Password: " + passwords[number - 1]);
                found = true;
            }
            number++;
        }

        if (found == false)
        {
            WriteLine("No matching account found.");
        }
    }
    // END SearchAccount

    // START UpdateAccount
    static void UpdateAccount()
    {
        if (total == 0)
        {
            WriteLine("No accounts to update.");
        }
        else
        {
            ViewAccounts();
            Write("Enter account number: ");
            int accountNumber;
            bool isNumber = int.TryParse(ReadLine(), out accountNumber);

            if (isNumber == false || accountNumber < 1 || accountNumber > total)
            {
                WriteLine("Invalid account number.");
            }
            else
            {
                Write("Enter new username (leave empty to keep): ");
                string newUsername = ReadLine();
                Write("Enter new password (leave empty to keep): ");
                string newPassword = ReadLine();

                if (newUsername != "")
                {
                    usernames[accountNumber - 1] = newUsername;
                }
                if (newPassword != "")
                {
                    passwords[accountNumber - 1] = newPassword;
                }
                WriteLine("Account updated successfully.");
            }
        }
    }
    // END UpdateAccount

    // START DeleteAccount
    static void DeleteAccount()
    {
        if (total == 0)
        {
            WriteLine("No accounts to delete.");
        }
        else
        {
            ViewAccounts();
            Write("Enter account number: ");
            int accountNumber;
            bool isNumber = int.TryParse(ReadLine(), out accountNumber);

            if (isNumber == false || accountNumber < 1 || accountNumber > total)
            {
                WriteLine("Invalid account number.");
            }
            else
            {
                WriteLine("Are you sure you want to delete this account? (Y/N)");
                string confirm = ReadYesNo();

                if (confirm == "Y")
                {
                    // Shift the remaining accounts one place to the left
                    for (int i = accountNumber - 1; i < total - 1; i++)
                    {
                        platforms[i] = platforms[i + 1];
                        usernames[i] = usernames[i + 1];
                        passwords[i] = passwords[i + 1];
                    }
                    total--;
                    WriteLine("Account deleted successfully.");
                }
                else
                {
                    WriteLine("Deletion cancelled.");
                }
            }
        }
    }
    // END DeleteAccount

    // START GeneratePassword
    static string GeneratePassword()
    {
        WriteLine("Enter password length (8 to 32):");
        int length;
        bool isNumber = int.TryParse(ReadLine(), out length);
        while (isNumber == false || length < 8 || length > 32)
        {
            WriteLine("Invalid length. Enter a number from 8 to 32.");
            isNumber = int.TryParse(ReadLine(), out length);
        }

        WriteLine("Include letters? (Y/N)");
        string useLetters = ReadYesNo();
        WriteLine("Include numbers? (Y/N)");
        string useNumbers = ReadYesNo();
        WriteLine("Include symbols? (Y/N)");
        string useSymbols = ReadYesNo();

        if (useLetters == "N" && useNumbers == "N" && useSymbols == "N")
        {
            WriteLine("Select at least one type. Letters will be used.");
            useLetters = "Y";
        }

        string pool = "";
        if (useLetters == "Y")
        {
            pool = pool + "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        }
        if (useNumbers == "Y")
        {
            pool = pool + "0123456789";
        }
        if (useSymbols == "Y")
        {
            pool = pool + "!@#$%^&*()-_=+";
        }

        string password = "";
        int count = 1;
        while (count <= length)
        {
            char letter = pool[rng.Next(pool.Length)];
            password = password + letter;
            count++;
        }
        return password;
    }
    // END GeneratePassword

    // START ReadYesNo
    static string ReadYesNo()
    {
        string answer = ReadLine().ToUpper();
        while (answer != "Y" && answer != "N")
        {
            WriteLine("Invalid answer. Enter Y or N.");
            answer = ReadLine().ToUpper();
        }
        return answer;
    }
    // END ReadYesNo
}
