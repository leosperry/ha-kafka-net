using KafkaFlow;
using KafkaFlow.Producers;

namespace HaKafkaNet;

/// <summary>
/// Moves raw ha events into new topic to more easily manage
/// </summary>
public class HaTransformerHandler : IMessageHandler<HaEntityState>
{
    IProducerAccessor _producerAccessor;
    IMessageProducer _producer;

    public HaTransformerHandler(IProducerAccessor producerAccessor)
    {
        _producerAccessor = producerAccessor;
        _producer = _producerAccessor.GetProducer("ha-producer");
    }

    public Task Handle(IMessageContext context, HaEntityState message)
    {
        return _producer.ProduceAsync(message.EntityId, message);
    }

}
