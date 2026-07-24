using Microsoft.AspNetCore.SignalR;

namespace CvManagement.Web.Hubs;

public class DiscussionHub : Hub
{
    public async Task JoinPositionGroup(string positionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"position-{positionId}");
    }

    public async Task LeavePositionGroup(string positionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"position-{positionId}");
    }
}
