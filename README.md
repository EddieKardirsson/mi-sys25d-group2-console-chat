# RetroChat

### Description 
RetroChat is a console-based chat client developed in C# that connects to an existing Socket.IO server for real-time communication. Users log in with username and navigate through a command-driven menu to participate either in a shared general chat or in dedicated chat rooms. The client supports real-time messaging, persists chat histories keyed to unique user GUIDs and provides notifications when users join or leaves.

### Features
- User authentication via username input and connection to the server.
- Acess to an open shared `General chat` and two dedicated chat rooms (`Room 1` and `Rooom 2`).
- Real-time message exchange with timestamps and sender details.
- Notifications for users joining and leaving chat rooms.
- Persistent local storage of chat history, stored separately per user with their unique GUID, which ensure chat data continuity across sessions. 

### How to run 
#### 1. Build and execute via command line
Open PowerShell or Command Prompt in the root directory of the project and run the following commands:
```bash
dotnet build
dotnet run --project RetroChat.csproj
``` 

#### 2. Using the interactive menu
1. Launch the application. 
2. Provide a username when prompted.
3. Use the command-driven menu to navigate between chat rooms and send messages.
 - Option `1` - Join the General Chat
 - Option `2` - Chat Rooms submenu 
     (choose `1` for Room 1 or `2` for Room 2)
 - Option `3` - Send Direct Message (under construction)
 - Option `Q` - Quit the program

While in a chat session you can type your message and press enter to send. Messages displays with timestamps and sender info. In the chat session you will also receive notifications when users enter or leave the room. It is possible to open multiple instances in seperate console windows with different usernames to test messaging. 
> **Note:** When using the interactive menu, ensure that your terminal window is high enough to display chat messages properly. If the console window is too small the application may crash or display errors. 

### In-Chat Commands
- `/quit` or `/exit` - Disconnects from the server and exits the application.

### Prereguisites
- .Net SDK 6.0 or later installed 
- Internet connection to the configured Websocket server and namespace endpoint.