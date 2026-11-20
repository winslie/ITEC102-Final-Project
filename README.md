# ITEC 102 Final Project

## Local password manager and password generator

This project is a C# console application for keeping account records in one place and generating random passwords. It was prepared for the ITEC 102 final project by:

- Winslie Ghlen A. Aragones
- Jared A. Manito
- Khen Joshki D. Rafael

## What the program does

The program presents a numbered console menu that repeats until the user chooses **Exit**. It supports:

1. **Add Account** - enter a platform name and username, then enter a password or generate one.
2. **View All Accounts** - display the saved records in a numbered list.
3. **Search Account** - find records by platform name without treating uppercase and lowercase letters as different.
4. **Update Account** - choose a record and replace its username or password. Leaving a replacement field empty keeps the existing value.
5. **Delete Account** - choose a record and confirm the deletion with `Y` or `N`.
6. **Generate Random Password** - choose a length from 8 to 32 characters and choose whether to include letters, numbers, and symbols.
7. **Exit** - save the current account list and close the program.

The application validates empty fields, menu choices, account numbers, password length, and `Y`/`N` answers. If no character type is selected during password generation, the program falls back to letters.

## Data file and security limitation

The program loads saved records when it starts and saves them to `accounts.txt` when it exits. The current implementation stores the platform, username, and password as **plain text**. It does not encrypt the file.

Use test accounts and test passwords only. Do not store banking, school, work, or other sensitive credentials in `accounts.txt`. Anyone who can open the file can read the saved passwords.

## Running the project

1. Open the C# project in Visual Studio or another environment that supports the project’s .NET version.
2. Build the project.
3. Run the console application.
4. Use the numbered menu and follow the prompts.
5. Choose **7. Exit** to save the current records to `accounts.txt`.

The data file is created in the program’s working directory when the program exits with saved records. Keep the file beside the program if you need the application to load the same records on its next run.

## Typical workflow

To add a generated password, choose **1. Add Account**, enter the platform and username, and select the option to generate a random password. To create a password separately, choose **6. Generate Random Password**. The generated value is printed in the console so it can be used when adding an account.

To change a record, choose **4. Update Account**, select its number, and enter a replacement username or password. To remove a record, choose **5. Delete Account**, select its number, and answer the confirmation prompt.

## Project scope

This is a local, single-user console application. It does not provide a graphical, web, or mobile interface; cloud synchronization; multi-device access; browser autofill; two-factor authentication; biometric login; or encrypted password storage.

## User manual

See [User-manual.pdf](documentation/User-manual.pdf) for the step-by-step guide and screenshots of the application running in Visual Studio.



