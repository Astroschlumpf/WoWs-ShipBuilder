using System;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.HttpClients;
using WoWsShipBuilder.Core.HttpResponses;
using WoWsShipBuilder.UI.Translations;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class SplashScreenViewModel : ViewModelBase
    {
        private const int TaskNumber = 4;

        private double progress;

        private AwsClient awsClient;

        private IFileSystem fileSystem;

        public SplashScreenViewModel()
            : this(AwsClient.Instance, new FileSystem())
        {
        }

        public SplashScreenViewModel(AwsClient awsClient, IFileSystem fileSystem)
        {
            this.awsClient = awsClient;
            this.fileSystem = fileSystem;
        }

        public double Progress
        {
            get => progress;
            set => this.RaiseAndSetIfChanged(ref progress, value);
        }

        private string downloadInfo = "placeholder";

        public string DownloadInfo
        {
            get => downloadInfo;
            set => this.RaiseAndSetIfChanged(ref downloadInfo, value);
        }

        public async Task VersionCheck()
        {
            IProgress<(int, string)> progressTracker = new Progress<(int state, string title)>(value =>
            {
                // ReSharper disable once PossibleLossOfFraction
                Progress = value.state * 100 / TaskNumber;
                DownloadInfo = value.title;
            });

            var today = DateTime.Today;
            Logging.GetLogger("VersionCheck").Info($"Last data version Check was at: {AppData.Settings.LastDataUpdateCheck}");
            if ((AppData.Settings.LastDataUpdateCheck == null || (AppData.Settings.LastDataUpdateCheck - today).Value.TotalDays > 1) || AppData.IsDebug)
            {
                AppData.Settings.LastDataUpdateCheck = DateTime.Today;
                var (shouldUpdate, canDeltaUpdate, newVersion) = await JsonVersionCheck(progressTracker);
                if (shouldUpdate)
                {
                    await DownloadImages(progressTracker, canDeltaUpdate, newVersion);
                }
            }

            progressTracker.Report((TaskNumber, Translation.SplashScreen_Done));
        }

        private async Task<(bool ShouldUpdate, bool CanDeltaUpdate, string NewVersion)> JsonVersionCheck(IProgress<(int, string)> progressTracker)
        {
            progressTracker.Report((1, Translation.SplashScreen_Json));
            return await awsClient.CheckFileVersion(AppData.Settings.SelectedServerType);
        }

        private async Task DownloadImages(IProgress<(int, string)> progressTracker, bool canDeltaUpdate, string newVersion)
        {
            string imageBasePath = fileSystem.Path.Combine(AppDataHelper.Instance.AppDataImageDirectory);
            var shipImageDirectory = fileSystem.DirectoryInfo.FromDirectoryName(fileSystem.Path.Combine(imageBasePath, "Ships"));
            if (!shipImageDirectory.Exists || !shipImageDirectory.GetFiles().Any() || !canDeltaUpdate)
            {
                progressTracker.Report((2, Translation.SplashScreen_ShipImages));
                await awsClient.DownloadImages(ImageType.Ship);
            }
            else
            {
                progressTracker.Report((2, Translation.SplashScreen_ShipImages));
                await awsClient.DownloadImages(ImageType.Ship, newVersion);
            }

            var camoImageDirectory = fileSystem.DirectoryInfo.FromDirectoryName(fileSystem.Path.Combine(imageBasePath, "Camos"));
            if (!camoImageDirectory.Exists || !camoImageDirectory.GetFiles().Any() || !canDeltaUpdate)
            {
                progressTracker.Report((3, Translation.SplashScreen_CamoImages));
                await awsClient.DownloadImages(ImageType.Camo);
            }
            else
            {
                progressTracker.Report((3, Translation.SplashScreen_CamoImages));
                await awsClient.DownloadImages(ImageType.Camo, newVersion);
            }
        }
    }
}
