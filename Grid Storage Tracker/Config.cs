using Torch;
using Torch.Views;

namespace GridStorageTracker
{
    public class Config : ViewModel
    {
        private bool _enableDedicatedLogFile = false;

        [Display(Name = "Enable Dedicated Log File",
            Description = "When enabled, writes a separate " +
            "'grid storage tracker.log' file. When disabled, " +
            "events still appear in Torch's main log — " +
            "useful on low-population servers where a dedicated file isn't needed."
            )]
        public bool EnableDedicatedLogFile
        {
            get => _enableDedicatedLogFile;
            set
            {
                _enableDedicatedLogFile = value;
                OnPropertyChanged(nameof(EnableDedicatedLogFile));
            }
        }
        [Display(Name = "Instructions", GroupName = "Info")]
        public string Instructions =>
            "Commands available in-game:\n" +
            "!gridstorage list 'PlayerName or SteamID' — shows a list of all grids stored in player's Service Terminal.\n" +
            "!gridstorage delete 'PlayerName or SteamID' 'index number shown in the list' — deletes a specific grid from that player's storage.";
    }
}