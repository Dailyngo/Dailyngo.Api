namespace EveryDaily.Application.Dtos.Message.Response;

public class MessageHubDto
{
    public string messageId { get; set; }
    public Guid SenderId { get; set; }
    public string Message { get; set; }
    public DateTimeOffset SendDate { get; set; }
}