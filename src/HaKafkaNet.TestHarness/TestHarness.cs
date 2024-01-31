using Microsoft.Extensions.Logging;
using Moq;

namespace HaKafkaNet;

/// <summary>
/// Provides a means of doing component level testing.
/// Internally, uses the Automation Manager to trigger your 
/// automations the same as they would at run time.
/// Exposes mock objects for you to use for service calls etc.
/// </summary>
public class TestHarness
{
    public Mock<IHaEntityProvider> EntityProvider { get; private set; }
    public Mock<IHaApiProvider> ApiProvider { get; private set; }
    public Mock<IHaStateCache> Cache { get; private set; }
    public Mock<IHaServices> Services { get; private set; }

    public IAutomationFactory Factory { get; private set; }
    public IAutomationBuilder Builder { get; private set; }

    static Mock<ILogger<AutomationManager>> _logger = new();
    static Mock<ILogger<ConditionalAutomationWrapper>> _wrapperLogger = new();

    IAutomationManager? _autoMgr;

    /// <summary>
    ///     /// Sets up the harness so that you can get the mock services.
    /// You must call Initialize with one of the available overloads to 
    /// set up your automations.
    /// </summary>
    public TestHarness()
    {
        ApiProvider = new();
        Cache = new();
        EntityProvider = new();
        Services = new();
        Services.Setup(s => s.Api).Returns(ApiProvider.Object);
        Services.Setup(s => s.Cache).Returns(Cache.Object);
        Services.Setup(s => s.EntityProvider).Returns(EntityProvider.Object);  

        Factory = new AutomationFactory(Services.Object, _wrapperLogger.Object);
        Builder = new AutomationBuilder(Services.Object);
    }

    public void Initialize(IAutomation automation)
    {
        _autoMgr = new AutomationManager(
            [automation], 
            Enumerable.Empty<IConditionalAutomation>(), 
            Enumerable.Empty<IAutomationRegistry>(), 
            _logger.Object);
    }

    public void Initialize(IConditionalAutomation automation)
    {
        _autoMgr = new AutomationManager(
            Enumerable.Empty<IAutomation>(),
            [automation],
            Enumerable.Empty<IAutomationRegistry>(),
            _logger.Object
        );
    }

    public void Initialize(IAutomationRegistry registry)
    {
        _autoMgr = new AutomationManager(
            Enumerable.Empty<IAutomation>(),
            Enumerable.Empty<IConditionalAutomation>(),
            [registry],
            _logger.Object
        );
    }

    /// <summary>
    /// You should call this in the 'Act' or 'When' portion of your test
    /// </summary>
    /// <param name="state"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Task SendState(HaEntityState state ,CancellationToken cancellationToken = default)
    {
        if (_autoMgr is null)
        {
            throw new Exception("TestHarness not initialized");
        }
        return _autoMgr.TriggerAutomations(new HaEntityStateChange()
        {
            EntityId = state.EntityId,
            New = state,
            EventTiming = EventTiming.PostStartup
        }, cancellationToken);
    }

    /// <summary>
    /// You should call this in the 'Act' or 'When' portion of your test
    /// </summary>
    /// <param name="state"></param>
    /// <param name="timing"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Task SendState(HaEntityState state, EventTiming timing, CancellationToken cancellationToken = default)
    {
        if (_autoMgr is null)
        {
            throw new Exception("TestHarness not initialized");
        }

        return _autoMgr.TriggerAutomations(new HaEntityStateChange()
        {
            EntityId = state.EntityId,
            New = state,
            EventTiming = timing
        }, cancellationToken);
    }

    /// <summary>
    /// You should call this in the 'Act' or 'When' portion of your test
    /// </summary>
    /// <param name="stateChange"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Task SendState(HaEntityStateChange stateChange, CancellationToken cancellationToken = default)
    {
        if (_autoMgr is null)
        {
            throw new Exception("TestHarness not initialized");
        }

        return _autoMgr.TriggerAutomations(stateChange, cancellationToken);
    }

    public void EnableAllAutomations()
    {
        foreach (var auto in _autoMgr!.GetAll())
        {
            _autoMgr.EnableAutomation(auto.GetMetaData().Id, true);
        }
    }
}
