using Microsoft.AspNetCore.SignalR;

namespace SWD.F_LocalBrand.API.Hubs;
public sealed class MessageHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined");
    }


    public async Task SendOrderHistoryUpdate(string orderHistoryUpdate, object orderHistoryUpdateMessage)
    {
        await Clients.All.SendAsync(orderHistoryUpdate, orderHistoryUpdateMessage);
    }

    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} + {message}");
    }

    public async Task SendOrderHistoryUpdateToCaller(string orderHistoryUpdate, object orderHistoryUpdateMessage)
    {
        await Clients.Caller.SendAsync(orderHistoryUpdate, orderHistoryUpdateMessage);
    }


    public async Task SendOrderHistoryUpdateToCustomer(string customerId, string orderHistoryUpdateMessage)
    {
        await Clients.User(customerId).SendAsync("ReceiveOrderHistoryUpdate", orderHistoryUpdateMessage);
    }


    public async Task SendOrderHistoryUpdateToConnection(string connectionId, string orderHistoryUpdateMessage)
    {
        await Clients.Client(connectionId).SendAsync("ReceiveOrderHistoryUpdate", orderHistoryUpdateMessage);
    }
    public async Task SendOrderHistoryUpdateToGroup(string groupName, string orderHistoryUpdateMessage)
    {
        await Clients.Group(groupName).SendAsync("ReceiveOrderHistoryUpdate", orderHistoryUpdateMessage);
    }
    public async Task AddToGroup(string groupName)
    => await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

    public async Task RemoveFromGroup(string groupName)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
}

