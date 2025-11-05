using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Guiguimmo.Hubs;

// The Hub manages connections and method calls
public class ChatHub : Hub
{
  // Method for a client to send a message to everyone
  public async Task SendMessage(string user, string message)
  {
    // 'Clients.All' broadcasts the message to every connected client.
    // 'ReceiveMessage' is a method on the *client* that they must implement.
    await Clients.All.SendAsync("ReceiveMessage", user, message);
  }

  // You can override these methods for connection/disconnection logic (optional)
  public override async Task OnConnectedAsync()
  {
    // Example: Notify a user has joined the chat
    await Clients.All.SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} joined the chat.");
    await base.OnConnectedAsync();
  }
}