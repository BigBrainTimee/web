using System.Diagnostics.Tracing;
using System.Fabric;
using Microsoft.ServiceFabric.Services.Runtime;

namespace TravelService;

[EventSource(Name = "MyCompany-web_projekat-TravelService")]
internal sealed class ServiceEventSource : EventSource
{
    public static readonly ServiceEventSource Current = new();

    static ServiceEventSource()
    {
        Task.Run(() => { });
    }

    private ServiceEventSource() : base() { }

    public static class Keywords
    {
        public const EventKeywords ServiceInitialization = (EventKeywords)0x2L;
    }

    [NonEvent]
    public void ServiceMessage(ServiceContext serviceContext, string message, params object[] args)
    {
        if (IsEnabled())
        {
            ServiceMessage(
                serviceContext.ServiceName.ToString(),
                serviceContext.ServiceTypeName,
                GetReplicaOrInstanceId(serviceContext),
                serviceContext.PartitionId,
                serviceContext.CodePackageActivationContext.ApplicationName,
                serviceContext.CodePackageActivationContext.ApplicationTypeName,
                serviceContext.NodeContext.NodeName,
                string.Format(message, args));
        }
    }

    private const int ServiceMessageEventId = 2;
    [Event(ServiceMessageEventId, Level = EventLevel.Informational, Message = "{7}")]
    private void ServiceMessage(
        string serviceName,
        string serviceTypeName,
        long replicaOrInstanceId,
        Guid partitionId,
        string applicationName,
        string applicationTypeName,
        string nodeName,
        string message)
    {
        WriteEvent(ServiceMessageEventId, serviceName, serviceTypeName, replicaOrInstanceId, partitionId, applicationName, applicationTypeName, nodeName, message);
    }

    private const int ServiceTypeRegisteredEventId = 3;
    [Event(ServiceTypeRegisteredEventId, Level = EventLevel.Informational, Message = "Service host process {0} registered service type {1}", Keywords = Keywords.ServiceInitialization)]
    public void ServiceTypeRegistered(int hostProcessId, string serviceType)
    {
        WriteEvent(ServiceTypeRegisteredEventId, hostProcessId, serviceType);
    }

    private const int ServiceHostInitializationFailedEventId = 4;
    [Event(ServiceHostInitializationFailedEventId, Level = EventLevel.Error, Message = "Service host initialization failed", Keywords = Keywords.ServiceInitialization)]
    public void ServiceHostInitializationFailed(string exception)
    {
        WriteEvent(ServiceHostInitializationFailedEventId, exception);
    }

    private static long GetReplicaOrInstanceId(ServiceContext context)
    {
        if (context is StatelessServiceContext stateless)
        {
            return stateless.InstanceId;
        }

        if (context is StatefulServiceContext stateful)
        {
            return stateful.ReplicaId;
        }

        throw new NotSupportedException("Context type not supported.");
    }
}
