using KafkaFlow;
using KafkaFlow.Middlewares.Serializer.Resolvers;

namespace HaKafkaNet;

public class HaMessageResolver : IMessageTypeResolver
{
    public  ValueTask<Type> OnConsumeAsync(IMessageContext context)
    {
        return ValueTask.FromResult(typeof(HaEntityState));
    }

    public ValueTask OnProduceAsync(IMessageContext context)
    {
        return ValueTask.CompletedTask;
    }
}
