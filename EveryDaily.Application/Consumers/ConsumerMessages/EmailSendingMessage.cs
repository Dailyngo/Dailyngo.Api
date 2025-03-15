namespace EveryDaily.Application.Consumers.ConsumerMessages;

public class EmailSendingMessage
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}