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

    // File that stores the master password (saved as a SHA-256 hash, not plain text)
    static string masterFile = "master.txt";
    static string masterHash = "";

    // BACK: becomes true when the user types 0 to go back to the menu
    static bool wentBack = false;

    // START PasswordManager
    static void Main()
    {
        string choice;
        bool isRunning = true;

        Title = "Password Manager"; // UI: text on the window title bar

        // Ask for the master password before showing the menu
        if (Login() == false)
        {
            ShowError("Too many wrong attempts. Program will close.");
            Pause();
            return;
        }

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
            ShowMenu(); // UI: boxed menu
            choice = ReadLine();
            Clear();
            wentBack = false;

            if (choice == "1")
            {
                ShowTitle("ADD ACCOUNT");
                ShowBackHint();
                AddAccount();
            }
            else if (choice == "2")
            {
                ShowTitle("ALL ACCOUNTS");
                ShowBackHint();
                ViewAccounts();
                RevealPassword(); // Pick an account number to show its password
            }
            else if (choice == "3")
            {
                ShowTitle("SEARCH ACCOUNT");
                ShowBackHint();
                SearchAccount();
            }
            else if (choice == "4")
            {
                ShowTitle("UPDATE ACCOUNT");
                ShowBackHint();
                UpdateAccount();
            }
            else if (choice == "5")
            {
                ShowTitle("DELETE ACCOUNT");
                ShowBackHint();
                DeleteAccount();
            }
            else if (choice == "6")
            {
                ShowTitle("GENERATE PASSWORD");
                ShowBackHint();
                string password = GeneratePassword();
                if (password == "")
                {
                    GoBack();
                }
                else
                {
                    WriteLine();
                    Write("Generated password: ");
                    ShowColored(password, ConsoleColor.White);
                    WriteLine();
                    ShowLevel(GetPasswordLevel(password, false));
                }
            }
            else if (choice == "7")
            {
                ShowTitle("RESET MASTER PASSWORD");
                ShowBackHint();
                ResetMasterPassword();
            }
            else if (choice == "8")
            {
                string[] lines = new string[total * 3];
                for (int i = 0; i < total; i++)
                {
                    lines[i * 3] = platforms[i];
                    lines[i * 3 + 1] = usernames[i];
                    lines[i * 3 + 2] = passwords[i];
                }
                System.IO.File.WriteAllLines("accounts.txt", lines);
                ShowSuccess("Accounts saved. Goodbye!");
                isRunning = false;
            }
            else
            {
                ShowError("Invalid choice. Please enter a number from 1 to 8.");
            }

            // BACK: if the user went back, go straight to the menu (no "Press any key")
            if (isRunning && wentBack == false)
            {
                Pause();
            }
        }
    }
    // END PasswordManager

    // START AddAccount
    static void AddAccount()
    {
        if (total == 100)
        {
            ShowError("Account list is full.");
            return;
        }

        Write("Enter platform: ");
        string platform = ReadLine();
        if (IsBack(platform)) { GoBack(); return; }
        while (platform == "")
        {
            ShowError("Platform name cannot be empty.");
            Write("Enter platform: ");
            platform = ReadLine();
            if (IsBack(platform)) { GoBack(); return; }
        }

        Write("Enter username: ");
        string username = ReadLine();
        if (IsBack(username)) { GoBack(); return; }
        while (username == "")
        {
            ShowError("Username cannot be empty.");
            Write("Enter username: ");
            username = ReadLine();
            if (IsBack(username)) { GoBack(); return; }
        }

        WriteLine();
        WriteLine("  [1] Enter my own password");
        WriteLine("  [2] Generate a random password");
        WriteLine("  [0] Back");
        Write("Enter option: ");
        string option = ReadLine();
        while (option != "1" && option != "2" && IsBack(option) == false)
        {
            ShowError("Invalid option. Enter 1, 2 or 0.");
            Write("Enter option: ");
            option = ReadLine();
        }
        if (IsBack(option)) { GoBack(); return; }
        WriteLine();

        string password;
        if (option == "2")
        {
            password = GeneratePassword();
            if (password == "") { GoBack(); return; }
            Write("Generated password: ");
            ShowColored(password, ConsoleColor.White);
            WriteLine();
        }
        else
        {
            Write("Enter password: ");
            password = ReadPassword();
            if (IsBack(password)) { GoBack(); return; }
            while (password == "")
            {
                ShowError("Password cannot be empty.");
                Write("Enter password: ");
                password = ReadPassword();
                if (IsBack(password)) { GoBack(); return; }
            }
            password = CheckAndSuggest(password); // Weak/strong check
            if (password == "") { GoBack(); return; }
        }

        platforms[total] = platform;
        usernames[total] = username;
        passwords[total] = password;
        total++;
        WriteLine();
        ShowSuccess("Account saved successfully.");
    }
    // END AddAccount

    // START ViewAccounts
    // Passwords are hidden with ******** and shown in a table
    static void ViewAccounts()
    {
        if (total == 0)
        {
            ShowWarning("No saved accounts.");
        }
        else
        {
            ShowTableHeader();
            int number = 1;
            while (number <= total)
            {
                ShowTableRow(number, "********");
                number++;
            }
            ShowLine();
            WriteLine("Total accounts: " + total);
        }
    }
    // END ViewAccounts

    // START RevealPassword
    // Lets the user pick an account number, asks the master password again, then shows the password
    static void RevealPassword()
    {
        if (total == 0)
        {
            return;
        }

        WriteLine();
        Write("Enter account number to show password (0 to go back): ");
        string input = ReadLine();
        if (IsBack(input)) { GoBack(); return; }

        int accountNumber;
        bool isNumber = int.TryParse(input, out accountNumber);

        if (isNumber == false || accountNumber < 1 || accountNumber > total)
        {
            ShowError("Invalid account number.");
        }
        else if (ConfirmMaster())
        {
            WriteLine();
            ShowTableHeader();
            ShowTableRow(accountNumber, passwords[accountNumber - 1]);
            ShowLine();
        }
    }
    // END RevealPassword

    // START SearchAccount
    // Passwords are hidden; user can pick a result to reveal
    static void SearchAccount()
    {
        Write("Enter keyword: ");
        string keyword = ReadLine();
        if (IsBack(keyword)) { GoBack(); return; }
        WriteLine();
        bool found = false;
        int number = 1;

        while (number <= total)
        {
            if (platforms[number - 1].ToLower().Contains(keyword.ToLower()))
            {
                if (found == false)
                {
                    ShowTableHeader(); // only print the header once
                }
                ShowTableRow(number, "********");
                found = true;
            }
            number++;
        }

        if (found == false)
        {
            ShowWarning("No matching account found.");
        }
        else
        {
            ShowLine();
            RevealPassword();
        }
    }
    // END SearchAccount

    // START UpdateAccount
    static void UpdateAccount()
    {
        if (total == 0)
        {
            ShowWarning("No accounts to update.");
            return;
        }

        ViewAccounts();
        WriteLine();
        Write("Enter account number (0 to go back): ");
        string input = ReadLine();
        if (IsBack(input)) { GoBack(); return; }

        int accountNumber;
        bool isNumber = int.TryParse(input, out accountNumber);

        if (isNumber == false || accountNumber < 1 || accountNumber > total)
        {
            ShowError("Invalid account number.");
            return;
        }

        if (ConfirmMaster() == false) // Master password again after choosing account
        {
            return;
        }

        WriteLine();
        Write("Enter new username (leave empty to keep): ");
        string newUsername = ReadLine();
        if (IsBack(newUsername)) { GoBack(); return; }

        Write("Enter new password (leave empty to keep): ");
        string newPassword = ReadPassword();
        if (IsBack(newPassword)) { GoBack(); return; }

        if (newPassword != "")
        {
            newPassword = CheckAndSuggest(newPassword); // Weak/strong check
            if (newPassword == "") { GoBack(); return; }
        }

        // Only save the changes at the end, so going back changes nothing
        if (newUsername != "")
        {
            usernames[accountNumber - 1] = newUsername;
        }
        if (newPassword != "")
        {
            passwords[accountNumber - 1] = newPassword;
        }
        WriteLine();
        ShowSuccess("Account updated successfully.");
    }
    // END UpdateAccount

    // START DeleteAccount
    static void DeleteAccount()
    {
        if (total == 0)
        {
            ShowWarning("No accounts to delete.");
            return;
        }

        ViewAccounts();
        WriteLine();
        Write("Enter account number (0 to go back): ");
        string input = ReadLine();
        if (IsBack(input)) { GoBack(); return; }

        int accountNumber;
        bool isNumber = int.TryParse(input, out accountNumber);

        if (isNumber == false || accountNumber < 1 || accountNumber > total)
        {
            ShowError("Invalid account number.");
            return;
        }

        if (ConfirmMaster() == false) // Master password again after choosing account
        {
            return;
        }

        ShowWarning("Are you sure you want to delete \"" + platforms[accountNumber - 1] + "\"? (Y/N, 0 = back)");
        string confirm = ReadYesNo();
        if (IsBack(confirm)) { GoBack(); return; }

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
            ShowSuccess("Account deleted successfully.");
        }
        else
        {
            ShowWarning("Deletion cancelled.");
        }
    }
    // END DeleteAccount

    // START GeneratePassword
    // Returns the new password, or "" if the user typed 0 to go back
    static string GeneratePassword()
    {
        Write("Enter password length (8 to 32): ");
        string input = ReadLine();
        if (IsBack(input)) return "";
        int length;
        bool isNumber = int.TryParse(input, out length);
        while (isNumber == false || length < 8 || length > 32)
        {
            ShowError("Invalid length. Enter a number from 8 to 32.");
            Write("Enter password length (8 to 32): ");
            input = ReadLine();
            if (IsBack(input)) return "";
            isNumber = int.TryParse(input, out length);
        }

        Write("Include letters? (Y/N): ");
        string useLetters = ReadYesNo();
        if (IsBack(useLetters)) return "";
        Write("Include numbers? (Y/N): ");
        string useNumbers = ReadYesNo();
        if (IsBack(useNumbers)) return "";
        Write("Include symbols? (Y/N): ");
        string useSymbols = ReadYesNo();
        if (IsBack(useSymbols)) return "";

        if (useLetters == "N" && useNumbers == "N" && useSymbols == "N")
        {
            ShowWarning("Select at least one type. Letters will be used.");
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
    // Returns "Y", "N", or "0" (back)
    static string ReadYesNo()
    {
        string answer = ReadLine().ToUpper();
        while (answer != "Y" && answer != "N" && IsBack(answer) == false)
        {
            ShowError("Invalid answer. Enter Y, N or 0 to go back.");
            Write("(Y/N): ");
            answer = ReadLine().ToUpper();
        }
        return answer;
    }
    // END ReadYesNo

    // START Login
    // First run: create a master password. Next runs: enter it (3 tries).
    // (No back option here because there is no menu yet.)
    static bool Login()
    {
        if (System.IO.File.Exists(masterFile) == false)
        {
            ShowTitle("CREATE MASTER PASSWORD");
            WriteLine("Welcome! Create a master password to protect your accounts.");
            WriteLine();
            string master = "";
            bool done = false;
            while (done == false)
            {
                Write("Create master password: ");
                master = ReadPassword();
                if (master == "")
                {
                    ShowError("Master password cannot be empty.");
                    continue;
                }

                master = CheckAndSuggest(master);
                if (master == "")
                {
                    continue; // user typed 0, so just ask again
                }

                Write("Confirm master password: ");
                string confirm = ReadPassword();
                if (confirm == master)
                {
                    done = true;
                }
                else
                {
                    ShowError("Passwords do not match. Try again.");
                    WriteLine();
                }
            }
            masterHash = HashText(master);
            System.IO.File.WriteAllText(masterFile, masterHash);
            WriteLine();
            ShowSuccess("Master password created.");
            Pause();
            return true;
        }

        masterHash = System.IO.File.ReadAllText(masterFile).Trim();
        ShowTitle("LOGIN");
        int tries = 3;
        while (tries > 0)
        {
            Write("Enter master password: ");
            string input = ReadPassword();
            if (HashText(input) == masterHash)
            {
                ShowSuccess("Login successful!");
                Pause();
                return true;
            }
            tries--;
            ShowError("Wrong master password. Tries left: " + tries);
        }
        return false;
    }
    // END Login

    // START ConfirmMaster
    // Asks the master password again before sensitive actions
    // Returns false if the password is wrong OR the user typed 0 to go back
    static bool ConfirmMaster()
    {
        Write("Enter master password to continue (0 to go back): ");
        string input = ReadPassword();
        if (IsBack(input))
        {
            GoBack();
            return false;
        }
        if (HashText(input) == masterHash)
        {
            return true;
        }
        ShowError("Wrong master password. Access denied.");
        return false;
    }
    // END ConfirmMaster

    // START ResetMasterPassword
    // Asks for the current master password, then lets the user set a new one
    static void ResetMasterPassword()
    {
        if (ConfirmMaster() == false)
        {
            return;
        }
        WriteLine();

        string newMaster = "";
        bool done = false;
        while (done == false)
        {
            Write("Enter new master password: ");
            newMaster = ReadPassword();
            if (IsBack(newMaster)) { GoBack(); return; }
            if (newMaster == "")
            {
                ShowError("Master password cannot be empty.");
                continue;
            }

            newMaster = CheckAndSuggest(newMaster);
            if (newMaster == "") { GoBack(); return; }

            Write("Confirm new master password: ");
            string confirm = ReadPassword();
            if (IsBack(confirm)) { GoBack(); return; }
            if (confirm == newMaster)
            {
                done = true;
            }
            else
            {
                ShowError("Passwords do not match. Try again.");
                WriteLine();
            }
        }

        masterHash = HashText(newMaster);
        System.IO.File.WriteAllText(masterFile, masterHash);
        WriteLine();
        ShowSuccess("Master password has been reset successfully.");
    }
    // END ResetMasterPassword

    // START ReadPassword
    // Reads typed characters but shows * instead of the real letters
    static string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key = ReadKey(true);
        while (key.Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Write("\b \b");
                }
            }
            else if (key.KeyChar != '\0' && char.IsControl(key.KeyChar) == false)
            {
                password = password + key.KeyChar;
                Write("*");
            }
            key = ReadKey(true);
        }
        WriteLine();
        return password;
    }
    // END ReadPassword

    // START GetPasswordLevel
    // Gives the password a level:
    //   WEAK        = shorter than 8 characters OR missing uppercase, lowercase, number or symbol
    //   STRONG      = at least 8 characters AND has uppercase, lowercase, number and symbol
    //   VERY STRONG = at least 12 characters AND has uppercase, lowercase, number and symbol
    // If showTips is true, it also prints what is missing.
    static string GetPasswordLevel(string password, bool showTips)
    {
        bool hasUpper = false;
        bool hasLower = false;
        bool hasDigit = false;
        bool hasSymbol = false;

        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else hasSymbol = true;
        }

        bool hasAllTypes = hasUpper && hasLower && hasDigit && hasSymbol;

        string level;
        if (password.Length >= 12 && hasAllTypes)
        {
            level = "VERY STRONG";
        }
        else if (password.Length >= 8 && hasAllTypes)
        {
            level = "STRONG";
        }
        else
        {
            level = "WEAK";
        }

        if (showTips)
        {
            if (level == "WEAK")
            {
                if (password.Length < 8) WriteLine("  - Must be at least 8 characters.");
                if (hasUpper == false) WriteLine("  - Add an uppercase letter (A-Z).");
                if (hasLower == false) WriteLine("  - Add a lowercase letter (a-z).");
                if (hasDigit == false) WriteLine("  - Add a number (0-9).");
                if (hasSymbol == false) WriteLine("  - Add a symbol (!@#$...).");
            }
            else if (level == "STRONG")
            {
                WriteLine("  - Tip: make it 12 characters or more to be VERY STRONG.");
            }
        }

        return level;
    }
    // END GetPasswordLevel

    // START CheckAndSuggest
    // Shows the password level (WEAK, STRONG, VERY STRONG).
    // If WEAK, suggests generating a stronger one instead.
    // Returns the password to use, or "" if the user typed 0 to go back.
    static string CheckAndSuggest(string password)
    {
        string level = GetPasswordLevel(password, true);
        ShowLevel(level);

        if (level == "VERY STRONG")
        {
            ShowSuccess("Excellent password!");
            return password;
        }
        if (level == "STRONG")
        {
            ShowSuccess("Good password.");
            return password;
        }

        // WEAK
        Write("Do you want to generate a strong password instead? (Y/N, 0 = back): ");
        string answer = ReadYesNo();
        if (IsBack(answer))
        {
            return "";
        }
        if (answer == "Y")
        {
            WriteLine("Tip: choose length 12+ and say Y to letters, numbers and symbols for VERY STRONG.");
            string generated = GeneratePassword();
            if (generated == "")
            {
                return "";
            }
            Write("Generated password: ");
            ShowColored(generated, ConsoleColor.White);
            WriteLine();
            ShowLevel(GetPasswordLevel(generated, false));
            ShowWarning("Please remember or write down this password.");
            return generated;
        }

        ShowWarning("Keeping your weak password.");
        return password;
    }
    // END CheckAndSuggest

    // START HashText
    // Turns text into a SHA-256 hash so the master password is not saved as plain text
    static string HashText(string text)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
        byte[] hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
    // END HashText

    // START IsBack
    // BACK: true if the user typed 0
    static bool IsBack(string input)
    {
        return input == "0";
    }
    // END IsBack

    // START GoBack
    // BACK: shows a message and tells Main to go back to the menu
    static void GoBack()
    {
        ShowWarning("Going back to the menu...");
        wentBack = true;
        System.Threading.Thread.Sleep(700); // short pause so the message can be seen
    }
    // END GoBack

    // START ShowBackHint
    static void ShowBackHint()
    {
        ShowColored("  (Type 0 at any time to go back to the menu)", ConsoleColor.DarkGray);
        WriteLine();
        WriteLine();
    }
    // END ShowBackHint

    // START ShowMenu
    static void ShowMenu()
    {
        ShowTitle("PASSWORD MANAGER");
        WriteLine("  Saved accounts: " + total);
        WriteLine();
        WriteLine("  [1] Add Account");
        WriteLine("  [2] View All Accounts");
        WriteLine("  [3] Search Account");
        WriteLine("  [4] Update Account");
        WriteLine("  [5] Delete Account");
        WriteLine("  [6] Generate Random Password");
        WriteLine("  [7] Reset Master Password");
        WriteLine("  [8] Save and Exit");
        WriteLine();
        ShowLine();
        Write("  Enter choice: ");
    }
    // END ShowMenu

    // START ShowTitle
    // Clears the screen and prints a title inside a box
    static void ShowTitle(string title)
    {
        Clear();
        ForegroundColor = ConsoleColor.Cyan;
        WriteLine("+==========================================+");
        WriteLine("|" + CenterText(title, 42) + "|");
        WriteLine("+==========================================+");
        ResetColor();
        WriteLine();
    }
    // END ShowTitle

    // START CenterText
    // Adds spaces on both sides so the text is in the middle
    static string CenterText(string text, int width)
    {
        int left = (width - text.Length) / 2;
        return text.PadLeft(left + text.Length).PadRight(width);
    }
    // END CenterText

    // START ShowLine
    static void ShowLine()
    {
        WriteLine("--------------------------------------------------------------------");
    }
    // END ShowLine

    // START ShowTableHeader
    static void ShowTableHeader()
    {
        ShowLine();
        ForegroundColor = ConsoleColor.Cyan;
        WriteLine(" " + "No.".PadRight(5) + "Platform".PadRight(20) + "Username".PadRight(25) + "Password");
        ResetColor();
        ShowLine();
    }
    // END ShowTableHeader

    // START ShowTableRow
    // number = account number (starts at 1), shownPassword = ******** or the real password
    static void ShowTableRow(int number, string shownPassword)
    {
        WriteLine(" " + number.ToString().PadRight(5) +
                  platforms[number - 1].PadRight(20) +
                  usernames[number - 1].PadRight(25) +
                  shownPassword);
    }
    // END ShowTableRow

    // START ShowLevel
    // Prints the password level in a color: WEAK = red, STRONG = yellow, VERY STRONG = green
    static void ShowLevel(string level)
    {
        Write("Password strength: ");
        if (level == "VERY STRONG")
        {
            ShowColored("[#####] VERY STRONG", ConsoleColor.Green);
        }
        else if (level == "STRONG")
        {
            ShowColored("[###  ] STRONG", ConsoleColor.Yellow);
        }
        else
        {
            ShowColored("[#    ] WEAK", ConsoleColor.Red);
        }
        WriteLine();
    }
    // END ShowLevel

    // START ShowColored
    // Writes text in a color, then goes back to the normal color
    static void ShowColored(string text, ConsoleColor color)
    {
        ForegroundColor = color;
        Write(text);
        ResetColor();
    }
    // END ShowColored

    // START ShowSuccess / ShowError / ShowWarning
    static void ShowSuccess(string message)
    {
        ShowColored("[OK] " + message, ConsoleColor.Green);
        WriteLine();
    }

    static void ShowError(string message)
    {
        ShowColored("[X] " + message, ConsoleColor.Red);
        WriteLine();
    }

    static void ShowWarning(string message)
    {
        ShowColored("[!] " + message, ConsoleColor.Yellow);
        WriteLine();
    }
    // END ShowSuccess / ShowError / ShowWarning

    // START Pause
    static void Pause()
    {
        WriteLine();
        ShowColored("Press any key to continue . . .", ConsoleColor.DarkGray);
        ReadKey(true);
    }
    // END Pause
}

