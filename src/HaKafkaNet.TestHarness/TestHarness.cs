﻿using System.Net;
using HaKafkaNet.Tests;
using Microsoft.AspNetCore.Http;
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
    static Mock<ILogger<AutomationWrapper>> _loggerReg = new();
    static Mock<ILogger<SystemObserver>> _observerLogger = new();
    static Mock<ILogger<DelayablelAutomationWrapper>> _wrapperLogger = new();

    IInternalRegistrar? _registrar;
    ISystemObserver? _observer;

    IAutomationManager? _autoMgr;

    /// <summary>
    /// Creates a new instance of the TestHarness.
    /// You must call Initialize with one of the available overloads to 
    /// set up your automations.
    /// </summary>
    /// <param name="defaultState">If specified, sets up non-generic get methods of services to return objects with the specified value as the state property</param>
    public TestHarness(string? defaultState = null)
    {
        ApiProvider = new();
        Cache = new();
        EntityProvider = new();
        Services = new();
        Services.Setup(s => s.Api).Returns(ApiProvider.Object);
        Services.Setup(s => s.Cache).Returns(Cache.Object);
        Services.Setup(s => s.EntityProvider).Returns(EntityProvider.Object);

        Factory = new AutomationFactory(Services.Object);
        Builder = new AutomationBuilder(Services.Object);

        if (defaultState is not null)
        {
            ApiProvider.Setup(ep => ep.GetEntity(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string entityId, CancellationToken _) => (new HttpResponseMessage(HttpStatusCode.OK), TestHelpers.GetState(entityId, defaultState)));

            Func<string, CancellationToken, HaEntityState> valueFunction = (string entityId, CancellationToken _) => TestHelpers.GetState(entityId, defaultState);

            EntityProvider.Setup(ep => ep.GetEntity(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(valueFunction);

            Cache.Setup(c => c.GetEntity(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(valueFunction);
        }
    }

    public void Initialize(IAutomation automation)
    {
        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());

        _registrar = new AutomationRegistrar([automation],Enumerable.Empty<IConditionalAutomation>(),Enumerable.Empty<ISchedulableAutomation>(), trace.Object, _loggerReg.Object);


        _autoMgr = new AutomationManager(
            Enumerable.Empty<IAutomationRegistry>(),
            _registrar);
    }

    public void Initialize(IConditionalAutomation automation)
    {
        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());

        _registrar = new AutomationRegistrar(
            Enumerable.Empty<IAutomation>(),
            [automation],
            Enumerable.Empty<ISchedulableAutomation>(), 
            trace.Object, _loggerReg.Object);
        
        _autoMgr = new AutomationManager(
            Enumerable.Empty<IAutomationRegistry>(),
            _registrar);
    }

    public void Initialize(IAutomationRegistry registry)
    {
        Mock<ISystemObserver> observer = new();
        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());

        _registrar = new AutomationRegistrar(
            Enumerable.Empty<IAutomation>(),
            Enumerable.Empty<IConditionalAutomation>(),
            Enumerable.Empty<ISchedulableAutomation>(), 
            trace.Object, _loggerReg.Object);
        
        _autoMgr = new AutomationManager(
            [registry],
            _registrar);
    }

    public void Initialize(
        IEnumerable<IAutomation>? automations = null, IEnumerable<IConditionalAutomation>? conditionals = null, IEnumerable<ISchedulableAutomation>? schedulables = null,
        IEnumerable<IAutomationRegistry>? registries = null, ISystemMonitor? monitor = null)
    {
        Mock<ISystemObserver> observer = new();
        Mock<IAutomationTraceProvider> trace = new();
        trace.Setup(t => t.Trace(It.IsAny<TraceEvent>(), It.IsAny<AutomationMetaData>(), It.IsAny<Func<Task>>()))
            .Callback<TraceEvent, AutomationMetaData, Func<Task>>((_, _, f) => f());
        
        if (monitor is null)
        {
            _observer = new Mock<ISystemObserver>().Object;
        }
        else
        {
            _observer = new SystemObserver([monitor], _observerLogger.Object);
        }

        _registrar = new AutomationRegistrar(
            automations ?? Enumerable.Empty<IAutomation>(),
            conditionals ?? Enumerable.Empty<IConditionalAutomation>(),
            schedulables ?? Enumerable.Empty<ISchedulableAutomation>(),
            trace.Object, _loggerReg.Object);
        
        _autoMgr = new AutomationManager(
            registries ?? Enumerable.Empty<IAutomationRegistry>(),
            _registrar);
    }

    [Obsolete("", false)]
    public void SetServiceGenericDefaults<T>(string state) where T : new()
    {
        SetServiceGenericDefaults<T>(state, new());
    }

    [Obsolete("SetServicesGenericDefaults", false)]
    public void SetServiceGenericDefaults<T>(string state, T atttributes)
    {
        ApiProvider.Setup(ep => ep.GetEntityState<T>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string entityId, CancellationToken _) => (new HttpResponseMessage(HttpStatusCode.OK), TestHelpers.GetState<T>(entityId, state, atttributes)));

        Func<string, CancellationToken, HaEntityState<T>> valueFunction = (string entityId, CancellationToken _) => TestHelpers.GetState<T>(entityId, state, atttributes);

        EntityProvider.Setup(ep => ep.GetEntityState<T>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(valueFunction);
        Cache.Setup(ep => ep.Get<T>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(valueFunction);
    }

    public void SetServicesGenericDefaults<Tstate, Tatt>(Tstate state, Tatt attributes)
    {
        ApiProvider.Setup(api => api.GetEntity<HaEntityState<Tstate,Tatt>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string entityId, CancellationToken _) => (new HttpResponseMessage(HttpStatusCode.OK), TestHelpers.GetEntity(entityId, state, attributes)));
        
        Func<string, CancellationToken, HaEntityState<Tstate, Tatt>> valueFunction = (string entityId, CancellationToken _) => TestHelpers.GetEntity(entityId, state, attributes);

        EntityProvider.Setup(ep => ep.GetEntity<HaEntityState<Tstate, Tatt>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(valueFunction);
        Cache.Setup(ep => ep.GetEntity<HaEntityState<Tstate, Tatt>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(valueFunction);
    }

    /// <summary>
    /// You should call this in the 'Act' or 'When' portion of your test
    /// </summary>
    /// <param name="state">The state to be assigned to New in the state change. Old will be null</param>
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
    /// <param name="state">The state to be assigned to New in the state change. Old will be null</param>
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
            _autoMgr.EnableAutomation(auto.GetMetaData().GivenKey, true);
        }
    }

    public void RaiseStateHandlerInitialized()
    {
        if (_observer is null)
        {
            throw new Exception("cannot raise event when no monitor is provided");
        }
        _observer.OnStateHandlerInitialized();
    }

    public void RaiseBadState(BadEntityState badState)
    {
        if (_observer is null)
        {
            throw new Exception("cannot raise event when no monitor is provided");
        }
        _observer.OnBadStateDiscovered(badState);
    }
}
