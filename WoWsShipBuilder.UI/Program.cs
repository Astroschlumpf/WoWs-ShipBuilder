using System;
using System.Globalization;
using System.Threading;
using Avalonia;
using Avalonia.ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.UI.Settings;

namespace WoWsShipBuilder.UI
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            Logging.InitializeLogging(ApplicationSettings.ApplicationOptions.SentryDsn);
            Logging.Logger.Info("Starting application...");
            var culture = new CultureInfo("en-GB");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception e)
            {
                Logging.Logger.Error(e);
                throw;
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseSkia()
                .UseReactiveUI();
    }
}
