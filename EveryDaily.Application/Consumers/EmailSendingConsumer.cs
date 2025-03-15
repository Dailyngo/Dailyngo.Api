using EveryDaily.Application.Consumers.ConsumerMessages;
using EveryDaily.Application.Services.Email;
using MassTransit;

namespace EveryDaily.Application.Consumers;

public class EmailSendingConsumer(IEmailService emailService)
    : IConsumer<EmailSendingMessage>
{
    public async Task Consume(ConsumeContext<EmailSendingMessage> context)
    {
        var message = context.Message;
        try
        {
            await emailService.SendAsync(message.To, message.Subject, message.Body);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}