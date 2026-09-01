using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace ChronosFlip;

public static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Mutex? appMutex = null;
        try
        {
            appMutex = new Mutex(initiallyOwned: true, @"Local\ChronosFlip.SingleInstance", out bool createdNew);
            if (!createdNew)
            {
                return;
            }

            Application.Start(_ =>
            {
                var context = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });
        }
        catch (AbandonedMutexException)
        {
            return;
        }
        finally
        {
            appMutex?.Dispose();
        }
    }
}