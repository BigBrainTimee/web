using Microsoft.ServiceFabric.Services.Runtime;
using QuestPDF.Infrastructure;
using System.Diagnostics;
using System.Threading;

namespace TravelService;

internal static class Program
{
    private static void Main()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        try
        {
            ServiceRuntime.RegisterServiceAsync("TravelServiceType",
                context => new TravelServiceHost(context)).GetAwaiter().GetResult();

            ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(TravelServiceHost).Name);
            Thread.Sleep(Timeout.Infinite);
        }
        catch (Exception e)
        {
            ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
            throw;
        }
    }
}
