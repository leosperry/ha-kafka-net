using Confluent.Kafka;
using HaKafkaNet;
using KafkaFlow;
using KafkaFlow.Producers;
using Moq;

namespace HaKafkaNet.Tests;

public class HaTransformerHandlerTests
{
    [Fact]
    async Task ShouldRepostMessageWithEntityIdAsKey()
    {
        Mock<IProducerAccessor> producerAccessor = new Mock<IProducerAccessor>();
        Mock<IMessageProducer> producer = new Mock<IMessageProducer>();
        producerAccessor.Setup(pa => pa.GetProducer("ha-producer")).Returns(producer.Object);
        
        HaTransformerHandler sut = new HaTransformerHandler(producerAccessor.Object);
        
        var fakeSate = TestHelpers.GetState();

        //act
        await sut.Handle(null!, fakeSate);

        //assert
        producerAccessor.Verify();
        producer.Verify(p => p.ProduceAsync("enterprise", fakeSate, null, null), Times.Once);
    }
}
