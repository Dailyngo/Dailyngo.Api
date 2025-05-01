namespace EveryDaily.Application.Dtos.Message.Response;

public class GetMessagesResponse
{
    public string Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Id { get; set; }
    public bool IsOwner { get; set; }
}