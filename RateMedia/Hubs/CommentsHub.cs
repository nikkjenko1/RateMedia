using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RateMedia.Hubs
{
        [Authorize]
        public class CommentsHub : Hub
        {
            public async Task SendCommentAdded(int movieId, object commentDto)
            {
                // broadcast to clients subscribed to movie group
                await Clients.Group($"movie-{movieId}").SendAsync("CommentAdded", commentDto);
            }

            public Task JoinMovieGroup(int movieId)
            {
                return Groups.AddToGroupAsync(Context.ConnectionId, $"movie-{movieId}");
            }

            public Task LeaveMovieGroup(int movieId)
            {
                return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"movie-{movieId}");
            }
        }
}
