using System;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HaKafkaNet;

internal interface IWrapperFactory
{
    IEnumerable<IAutomation> GetWrapped(IAutomationBase auto);
}

internal class WrapperFactory : IWrapperFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAutomationTraceProvider _trace;
    private readonly ISystemObserver _observer;
    private readonly ILogger<DelayablelAutomationWrapper> _logger;

    public WrapperFactory(
        IServiceProvider serviceProvider,
        IAutomationTraceProvider trace,
        ISystemObserver observer,
        ILogger<DelayablelAutomationWrapper> logger)
    {
        _serviceProvider = serviceProvider;
        _trace = trace;
        _observer = observer;
        _logger = logger;
    }

    public IEnumerable<IAutomation> GetWrapped(IAutomationBase auto)
    {
        if (auto is IDelayableAutomation delayable)
        {
            var newType = typeof(DelayablelAutomationWrapper<>).MakeGenericType([auto.GetType()]);

            yield return (IAutomation)ActivatorUtilities.CreateInstance(_serviceProvider, newType, auto);
        }

        var supportedInterfaceTypes = new HashSet<Type>()
        {
            typeof(IAutomation<,>),
            typeof(IDelayableAutomation<,>),
        };

        var concrete = auto.GetType();

        var typeIneterfaces = auto.GetType().GetInterfaces();
        var stronglyTypedAutomationDefinitions = typeIneterfaces.Where(i => 
            supportedInterfaceTypes.Contains(i) ||  (i.IsGenericType && supportedInterfaceTypes.Contains(i.GetGenericTypeDefinition()))
                ).ToArray();

        foreach (var targetInterface in stronglyTypedAutomationDefinitions)
        {
            var genericArgs = targetInterface.GetGenericArguments();

            Type? iautomationType;

            switch(targetInterface.GetGenericTypeDefinition())
            {
                case var x when x == typeof(IAutomation<>):
                    iautomationType = typeof(TypedAutomationWrapper<,,>).MakeGenericType([concrete, genericArgs[0], typeof(JsonElement)]);
                    yield return (IAutomation)ActivatorUtilities.CreateInstance(_serviceProvider, iautomationType, auto);;
                    break;
                case var x when x == typeof(IAutomation<,>):
                    iautomationType = typeof(TypedAutomationWrapper<,,>).MakeGenericType([concrete, genericArgs[0], genericArgs[1]]);
                    yield return (IAutomation)ActivatorUtilities.CreateInstance(_serviceProvider, iautomationType, auto);
                    break;
                case var x when x == typeof(IDelayableAutomation<,>):
                    var typedWrapperType = typeof(TypedDelayedAutomationWrapper<,,>).MakeGenericType([concrete, genericArgs[0], genericArgs[1]]);

                    IDelayableAutomation delayableAutomation = (IDelayableAutomation)ActivatorUtilities.CreateInstance(_serviceProvider, typedWrapperType, auto);

                    var newType = typeof(DelayablelAutomationWrapper<>).MakeGenericType([typedWrapperType]);

                    yield return (IAutomation)ActivatorUtilities.CreateInstance(_serviceProvider, newType, delayableAutomation);
                    break;
                default:
                    throw new HaKafkaNetException($"could not find appropriate wrapper: {concrete}");
            }
        }
    }
}
