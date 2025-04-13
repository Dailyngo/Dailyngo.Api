using EveryDaily.Application.Consumers.ConsumerMessages;
using EveryDaily.Application.Services.Badge;
using MassTransit;

namespace EveryDaily.Application.Consumers
{
    public class RankActivityConsumer : IConsumer<RankActivityMessage>
    {
        private readonly IRankService _rankService;

        public RankActivityConsumer(IRankService rankService)
        {
            _rankService = rankService;
        }

        public async Task Consume(ConsumeContext<RankActivityMessage> context)
        {
            var message = context.Message;

            try
            {
                await _rankService.ProcessActivityAsync(message.UserId, message.ActivityType, CancellationToken.None);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
