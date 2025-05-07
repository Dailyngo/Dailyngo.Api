namespace EveryDaily.Application.Dtos.Message.Response;

public class GetMessagesUsersResponse
{
    public string UserId { get; set; }
    public string FullName { get; set; }
    public string? UserImage { get; set; }
    public int UnreadCount { get; set; }
    public DateTimeOffset LastMessageDate { get; set; }
    public DateTimeOffset? LastMessageReadDate { get; set; }
    public string? LastMessage { get; set; }
    public bool LastMessageOwner { get; set; }
}