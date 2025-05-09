using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System;
using System.Text.RegularExpressions;
using wallspace;
using wallspace.Migrations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Runtime.Intrinsics.X86;

public class ChatHub : Hub //8 - arv
{
    private readonly AppDbContext _context;

    // Inject DbContext into the constructor
    public ChatHub(AppDbContext context)
    {
        _context = context;
    }

   
    public async Task<bool> CreateUser(string username, string password)
    {
        try
        {
        if(username.Length <= 0) { return false; }
        if(password.Length <= 0) { return false; }
        // Check if the user already exists
        if (GetUsersByUsername(username).Any())
        {
            return false;
        }

        // Step 1: Save the password first
        var newPassword = new Passwords
        {
            password = password
        };
        _context.Passwords.Add(newPassword);
        await _context.SaveChangesAsync(); // ID is now generated

        // Step 2: Use the password ID for the user
        var newUser = new Users
        {
            username = username,
            password_id = newPassword.Id
        };
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return true;

        }
        catch (Exception ex) //7
        {
            // Log to console or a logger
            Console.WriteLine($"Error in CreateUser: {ex.Message}");
            throw;
        }
    }


    public Task<List<Users>> GetUsersWithValidPassword(string username, string inputPassword)
    {
        // Step 1: Get all users with the given username
        List<Users> possibleUsers = _context.Users
            .Where(u => u.username == username)
            .ToList(); // Get all users with the given username

        // Step 2: Iterate over each user and check if the password matches
        List<Users> validUsers = new List<Users>();

        for (int i = 0; i < possibleUsers.Count; i++)
        {
            var user = possibleUsers[i];

            // Step 3: Retrieve the associated password using the user's PasswordId
            var userPassword = _context.Passwords
                .Where(p => p.Id == user.password_id)
                .FirstOrDefault(); // Get the password for this user

            if (userPassword != null && userPassword.password == inputPassword)
            {
                // Step 5: Add the user to the validUsers list if the password is correct
                validUsers.Add(user);
            }
        }

        // Return the list wrapped in a Task
        return Task.FromResult(validUsers);
    }


    public List<Users> GetUsersByUsername(string username)
    {
        // Query the database to find all users with the provided username
        var users = _context.Users
            .Where(u => u.username == username) // Filters users by username
            .ToList(); // Executes the query and returns a list of users

        return users;
    }

    public async Task<List<Messages>> GetMessagesFromChatroom(int roomID, int skip, int take)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomID.ToString());//something something SignalR Magic :Sparkels:

        try { 
        var messages = await _context.Messages
                                    .Where(p => p.chatroom_id == roomID)
                                    .OrderBy(m => m.time_stamp)
                                    .Skip(skip)
                                    .Take(take)
                                    .ToListAsync();

        return messages;  //\\//\\

        }
        catch (Exception ex)
        {
            // Log to console or a logger
            Console.WriteLine($"Error in GetMessagesFromChatroom: {ex.Message}"); //11 (overloaded method)
            throw;
        }

    }

    public async Task exitChatroomChannel(int roomID)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomID.ToString());
    }

    public async Task joinUserChannel(int userID)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "user_" + userID.ToString());
    }

    public async Task<bool> SendMessage(string message, int chatroom_id, int user_id)
    {
        var newMessage = new Messages//Creates a tempoary instance that get all its values set
        {
            message = message,
            chatroom_id = chatroom_id,
            user_id = user_id
        };
        _context.Messages.Add(newMessage);
        await _context.SaveChangesAsync();  //note to self, remember to safe ;)

        await Clients.Group(chatroom_id.ToString()).SendAsync("ReceiveMessage", message, user_id);

        return true;
    }
    


    public async Task<int> CreateChatroom(string name = "no name")
    {
        var newName = new Chatroom_Names
        {
            chatroom_name = name
        };

        _context.Chatroom_Names.Add(newName);

        await _context.SaveChangesAsync(); //So the ID generates

        var newChatroom = new Chatrooms
        {
            name_id = newName.Id
        };
        _context.Chatrooms.Add(newChatroom);
        await _context.SaveChangesAsync();

        return newChatroom.Id;//Gives the ID so you can easily use the newly generated server
    }

    public async Task<bool> isUserInChatroom(int userID, int chatRoomID)
    {
        //WARNING: DONT REMOVE ANY SPELLING ERRORS (EVEN IN COMMENTS), I WILL KNOW [ >:( ]  
        var user = await _context.Bind_Chatrooms_Users
                                    .Where(p => p.chatroom_id == chatRoomID)
                                    .Where(p => p.user_id == userID)
                                    .ToListAsync();

        if(user != null) { return true; } //Is a match is found

        return false; //No matches found :,C
    }

    public async Task<List<int>> getEachChatroomID_WhereUser(int userID)
    {
        try
        {
            var chatrooms = await _context.Bind_Chatrooms_Users
                                        .Where(p => p.user_id == userID)
                                        .OrderByDescending(m => m.Id)
                                        .Select(p => p.chatroom_id)
                                        .ToListAsync();

            return chatrooms;

        }
        catch (Exception ex) //7
        {
            // Log to console or a logger
            Console.WriteLine($"Error in getEachChatroomID_WhereUser: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> addUserToChatroom(int chatroomID, int userID)
    {
        try
        {
            var matchingChatroomBind = await _context.Bind_Chatrooms_Users
            .Where(p => p.chatroom_id == chatroomID && p.user_id == userID)
                                        .FirstOrDefaultAsync();

            if(matchingChatroomBind != null)//Means it already exist
            {
                return false;
            }


            var entry = new Bind_Chatrooms_Users
            {
                chatroom_id = chatroomID,
                user_id = userID
            };

            _context.Bind_Chatrooms_Users.Add(entry);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in addUserToChatroom: {ex.Message}");
            return false;
        }
    }


    public async Task<List<int>> getPrivateChatroomID(int user1) //Private chatroom means 1 to 1 chat, which is stored diffrently
    {
        try
        {
            var chatrooms = await _context.Bind_User_User
                                        .Where(p => p.user_1 == user1 || p.user_2 == user1)
                                        .OrderByDescending(m => m.Id)
                                        .Select(p => p.chatroom_id)
                                        .ToListAsync();

            return chatrooms;

        }
        catch (Exception ex)
        {
            // Log to console or a logger
            Console.WriteLine($"Error in getPrivateChatroomID: {ex.Message}");
            throw;
        }
    }

    public async Task<List<int>> getPrivateChatroomUsersByRoomID(int roomID) //Private chatroom means 1 to 1 chat, which is stored diffrently
    {
        try
        {
            var chatroom = await _context.Bind_User_User
                                        .Where(p => p.chatroom_id == roomID)

                                        .FirstOrDefaultAsync();
            if (chatroom == null)
            {
                return new List<int> { 0, 0 };
            }

            return new List<int> { chatroom.user_1, chatroom.user_2 };

        }
        catch (Exception ex)
        {
            // Log to console or a logger
            Console.WriteLine($"Error in getPrivateChatroomUsersByRoomID: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> createOneToOneChatroom(int user1, int user2) // Private chatroom means 1 to 1 chat, which is stored differently
    {
        // Check if the chatroom already exists
        var existingChatroom = await _context.Bind_User_User
            .Where(p => (p.user_1 == user1 && p.user_2 == user2) || (p.user_1 == user2 && p.user_2 == user1))
            .FirstOrDefaultAsync();

        if (existingChatroom != null)
        {
            return false; //Chatroom dosen't exist
        }
        if (user1 == user2)
        {
            return false; //The user shoudn't have a chatroom with themself
        }
        if (user1 == 0 || user2 == 0) { return false; }
        var newChatroom = new Chatrooms
        {
            name_id = 0
        };

        // Add new chatroom to the database
        _context.Chatrooms.Add(newChatroom);

        // Save changes to database
        await _context.SaveChangesAsync();

        // Create the relationship between the users and the chatroom
        var bindUserUser = new Bind_User_User
        {
            user_1 = user1,
            user_2 = user2,
            chatroom_id = newChatroom.Id
        };

        _context.Bind_User_User.Add(bindUserUser);
        await _context.SaveChangesAsync();

        await Clients.Group("user_" + user1.ToString()).SendAsync("updateChatroomList");
        await Clients.Group("user_" + user2.ToString()).SendAsync("updateChatroomList");


        return true;
    }

    public async Task<int> getUserID_byName(string userName)//If there are multiple user that share a name, that may be a problem
    {

        var user = await _context.Users
            .Where(p => (p.username == userName))
            .FirstOrDefaultAsync();

        if(user?.Id == null)//Just in case, may cause weird behavior^TM
        {
            return 0;
        }

        return user.Id;
    }

    public async Task<string> getUserName_byID(int id)//If there are multiple user that share a name, that may be a problem
    {

        var user = await _context.Users
            .Where(p => (p.Id == id))
            .FirstOrDefaultAsync();
        if(user == null)
        { return "Failed to load username"; }

        return user.username;
    }

    public async Task<List<int>> getChatroomID_byName(string name)
    {
        try
        {//////////////////|///////////////////
            var chatNames = await _context.Chatroom_Names
                                        .Where(p => p.chatroom_name == name)
                                        .Select(p => p.Id)
                                        .ToListAsync();


            var chatrooms = await _context.Chatrooms
                                        .Where(p => chatNames.Contains(p.name_id))
                                        .OrderByDescending(m => m.Id)
                                        .Select(p => p.Id)
                                        .ToListAsync();

            return chatrooms;

        }
        catch (Exception ex)
        {
            // Log to console or a logger
            Console.WriteLine($"Error in getEachChatroomID_WhereUser: {ex.Message}");
            throw;
        }
    }

    public async Task<string> getChatroomName_byID(int id)
    {
        try
        {//////////////////////////////////////

            var chatroomNameID = await _context.Chatrooms
                                        .Where(p => p.Id == id)
                                        .OrderByDescending(m => m.Id)
                                        .Select(p => p.name_id)
                                        .FirstOrDefaultAsync();


            var chatName = await _context.Chatroom_Names
                                        .Where(p => p.Id == chatroomNameID)
                                        .Select(p => p.chatroom_name)
                                        .FirstOrDefaultAsync();


            
            
            if(chatName == null)
            {
                return "";
            }
            return chatName;

        }
        catch (Exception ex)
        {
            // Log to console or a logger
            Console.WriteLine($"Error in getEachChatroomID_WhereUser: {ex.Message}");
            throw;
        }
    }



    public async Task<string> UploadImageFromBase64Async(string base64Image, string name, int userID)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(base64Image))
                throw new ArgumentException("Base64 image string is empty");

            // Ensure image directory exists
            var savePath = Path.Combine("wwwroot", "images");
            Directory.CreateDirectory(savePath);

            // Create a safe filename using timestamp and name
            var fileName = $"{name}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.png";
            fileName = Regex.Replace(fileName, @"[^a-zA-Z0-9_\.-]", "_"); // Sanitize

            var user = await _context.Users.FindAsync(userID);
            if (user == null)
            {
                throw new HubException("User not found.");
            }
            var Image = new Images
            {
                file_name = fileName
            };

            _context.Images.Add(Image);
            await _context.SaveChangesAsync();

            user.picture_id = Image.Id;

            await _context.SaveChangesAsync();

            var fullPath = Path.Combine(savePath, fileName);

            // Decode and save image
            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(base64Image);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid base64 string.");
            }

            await File.WriteAllBytesAsync(fullPath, imageBytes);

            return $"/images/{fileName}";
        }
        catch (Exception ex)
        {
            Console.WriteLine("Image upload failed: " + ex.Message);
            throw new HubException("Image upload failed: " + ex.Message);
        }

    }

    
    /*public async Task<string> getProfilePicture(int userID)
    {
        try
        { 
        var user = await _context.Users.FindAsync(userID);
        if (user == null)
            { 
            throw new Exception("User not found.");
            }
            var picture = await _context.Images.FindAsync(user.picture_id);
        if (picture == null)
            { 
            throw new Exception("Picture not found.");
            }


            return picture.file_name;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to fetch profile picture: " + ex);
            throw;
        }
    }
    */
}