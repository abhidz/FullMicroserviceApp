using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumer
{
    public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
    {
        public async Task Consume(ConsumeContext<AuctionDeleted> context)
        {
            await Console.Out.WriteLineAsync("Consuming auction deleted: " + context.Message.Id);
            var result = await DB.DeleteAsync<Item>(context.Message.Id);
            if (!result.IsAcknowledged)
            {
                throw new MessageException(typeof(AuctionDeleted), "Problem updating mongodb");
            }
        }
    }
}