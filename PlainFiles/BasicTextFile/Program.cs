using BasicTextFile;
using System.IO;
using System.Linq;
using System.Collections.Generic;

List<User> LoadUsers(string path)
{
    var users = new List<User>();
    if (!File.Exists(path)) return users;
    foreach (var line in File.ReadAllLines(path))
    {
        var parts = line.Split(',');
        if (parts.Length == 4)
        {
            users.Add(new User
            {
                Id = int.TryParse(parts[0], out var id) ? id : 0,
                Username = parts[1],
                Password = parts[2],
                IsActive = bool.TryParse(parts[3], out var active) && active
            });
        }
    }
    return users;
}

void SaveUsers(string path, List<User> users)
{
    var lines = users.Select(u => $"{u.Id},{u.Username},{u.Password},{u.IsActive}");
    File.WriteAllLines(path, lines);
}


const string usersFile = "Users.txt"; 
var users = LoadUsers(usersFile);

int attempts = 0;
User? loggedUser = null;
string? inputUsername = null;

while (attempts < 3 && loggedUser == null)
{
    Console.Write("Username: ");
    inputUsername = Console.ReadLine();
    Console.Write("Password: ");
    var inputPassword = Console.ReadLine();

    var user = users.FirstOrDefault(u => u.Username == inputUsername);

    if (user == null)
    {
        Console.WriteLine("User not found.");
        attempts++;
        continue;
    }
    if (!user.IsActive)
    {
        Console.WriteLine("User is blocked. Contact admin.");
        return;
    }
    if (user.Password == inputPassword)
    {
        loggedUser = user;
        break;
    }
    else
    {
        Console.WriteLine("Incorrect password.");
        attempts++;
    }
}

if (loggedUser == null)
{
    var user = users.FirstOrDefault(u => u.Username == inputUsername);
    if (user != null)
    {
        user.IsActive = false;
        SaveUsers(usersFile, users);
        Console.WriteLine("User has been blocked due to too many failed attempts.");
    }
    return;
}

var textFile = new SimpleTextFile("data.txt");
var lines = textFile.ReadLines();



void SaveChanges()
{
    Console.WriteLine("Saving changes...");
    textFile.WriteLines(lines);
    Console.WriteLine("Changes saved.");
}

using (var logger = new LogWriter("log.txt"))
{
    var opc = "0";
    logger.WriteLog("INFO", $"[{loggedUser.Username}] Application started.");
    do
    {
        opc = Menu();
        Console.WriteLine("=======================================");
        switch (opc)
        {
            case "1":
                logger.WriteLog("INFO", $"[{loggedUser.Username}] Show content.");
                if (lines.Length == 0)
                {
                    Console.WriteLine("Empty file.");
                    logger.WriteLog("ERROR", $"[{loggedUser.Username}] Empty file.");
                    break;
                }
                foreach (var line in lines)
                {
                    Console.WriteLine(line);
                }
                break;

            case "2":
                logger.WriteLog("INFO", $"[{loggedUser.Username}] Add line.");
                Console.Write("Enter the line to add: ");
                var newLine = Console.ReadLine();
                if (!string.IsNullOrEmpty(newLine))
                {
                    lines = lines.Append(newLine).ToArray();
                }
                break;

            case "3":
                logger.WriteLog("INFO", $"[{loggedUser.Username}] Update line.");
                Console.Write("Enter the line to update: ");
                var lineToUpdate = Console.ReadLine();
                Console.Write("Enter the new value: ");
                var newValue = Console.ReadLine();
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] == lineToUpdate)
                    {
                        lines[i] = newValue!;
                        break;
                    }
                }
                break;

            case "4":
                logger.WriteLog("INFO", $"[{loggedUser.Username}] Remove line.");
                Console.Write("Enter the line to remove: ");
                var lineToRemove = Console.ReadLine();
                if (!string.IsNullOrEmpty(lineToRemove))
                {
                    lines = lines.Where(line => line != lineToRemove).ToArray();
                }
                break;

            case "5":
                SaveChanges();
                logger.WriteLog("INFO", $"[{loggedUser.Username}] Save file.");
                break;

            case "6":
                logger.WriteLog("INFO", $"[{loggedUser.Username}] Edit user.");
                Console.Write("Enter the ID of the user to edit: ");
                if (int.TryParse(Console.ReadLine(), out int editId))
                {
                    var userToEdit = users.FirstOrDefault(u => u.Id == editId);
                    if (userToEdit == null)
                    {
                        Console.WriteLine("User not found.");
                        logger.WriteLog("ERROR", $"[{loggedUser.Username}] Tried to edit non-existent user with ID {editId}.");
                        break;
                    }

                    Console.WriteLine($"Current username: {userToEdit.Username}");
                    Console.Write("New username (ENTER to keep): ");
                    var newUsername = Console.ReadLine();
                    if (!string.IsNullOrEmpty(newUsername))
                        userToEdit.Username = newUsername;

                    Console.WriteLine($"Current password: {userToEdit.Password}");
                    Console.Write("New password (ENTER to keep): ");
                    var newPassword = Console.ReadLine();
                    if (!string.IsNullOrEmpty(newPassword))
                        userToEdit.Password = newPassword;

                    Console.WriteLine($"Current active status: {userToEdit.IsActive}");
                    Console.Write("New active status (true/false, ENTER to keep): ");
                    var newActive = Console.ReadLine();
                    if (!string.IsNullOrEmpty(newActive) && bool.TryParse(newActive, out bool isActive))
                        userToEdit.IsActive = isActive;

                    SaveUsers(usersFile, users);
                    Console.WriteLine("User updated.");
                    logger.WriteLog("INFO", $"[{loggedUser.Username}] Updated user with ID {editId}.");
                }
                else
                {
                    Console.WriteLine("Invalid ID.");
                    logger.WriteLog("ERROR", $"[{loggedUser.Username}] Entered invalid ID for edit.");
                }
                break;

                case "7":
    logger.WriteLog("INFO", $"[{loggedUser.Username}] Attempting to delete user.");
    Console.Write("Enter the ID of the user to delete: ");
    if (int.TryParse(Console.ReadLine(), out int deleteId))
    {
        var userToDelete = users.FirstOrDefault(u => u.Id == deleteId);
        if (userToDelete == null)
        {
            Console.WriteLine("User not found.");
            logger.WriteLog("ERROR", $"[{loggedUser.Username}] Tried to delete non-existent user with ID {deleteId}.");
            break;
        }

        // Muestra los datos del usuario
        Console.WriteLine($"ID: {userToDelete.Id}");
        Console.WriteLine($"Username: {userToDelete.Username}");
        Console.WriteLine($"Password: {userToDelete.Password}");
        Console.WriteLine($"Active: {userToDelete.IsActive}");

        Console.Write("Are you sure you want to delete this user? (y/n): ");
        var confirm = Console.ReadLine();
        if (confirm?.ToLower() == "y")
        {
            users.Remove(userToDelete);
            SaveUsers(usersFile, users);
            Console.WriteLine("User deleted.");
            logger.WriteLog("INFO", $"[{loggedUser.Username}] Deleted user with ID {deleteId}.");
        }
        else
        {
            Console.WriteLine("Operation cancelled.");
            logger.WriteLog("INFO", $"[{loggedUser.Username}] Cancelled deletion of user with ID {deleteId}.");
        }
    }
    else
    {
        Console.WriteLine("Invalid ID.");
        logger.WriteLog("ERROR", $"[{loggedUser.Username}] Entered invalid ID for delete.");
    }
    break;


            case "0":
                logger.WriteLog("INFO", $"[{loggedUser.Username}] Exiting application.");
                Console.WriteLine("Exiting...");
                break;

            default:
                logger.WriteLog("ERROR", $"[{loggedUser.Username}] Invalid option.");
                Console.WriteLine("Invalid option. Please try again.");
                break;
        }
    } while (opc != "0");
    logger.WriteLog("INFO", $"[{loggedUser.Username}] Application ended.");
    SaveChanges();
}

string Menu()
{
    Console.WriteLine("=======================================");
    Console.WriteLine("1. Show content");
    Console.WriteLine("2. Add line");
    Console.WriteLine("3. Update line");
    Console.WriteLine("4. Remove line");
    Console.WriteLine("5. Save changes");
    Console.WriteLine("6. Edit user");
    Console.WriteLine("7. Delete user");
    Console.WriteLine("0. Exit");
    Console.Write("Enter your option: ");
    return Console.ReadLine() ?? "0";
}