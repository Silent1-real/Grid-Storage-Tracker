using System.IO;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.Managers.PatchManager;
using Torch.Views;

namespace GridStorageTracker
{
    public class Core : TorchPluginBase ,IWpfPlugin
    {
        private VeryPersistent<Config> _config;

        // PatchManager is used to apply method patches to the game's code.
        private PatchManager _patchManager;
        private bool _patched;
 
        public override void Init(ITorchBase torch)
        {
            // Initialize the plugin and set up logging and configuration.
            base.Init(torch);
            string serverRoot = Directory.GetParent(torch.Config.InstancePath).FullName;
            string logFolder = Path.Combine(serverRoot, "Logs");

            // seting up the config file using rar very persisitent.
            string configPath = Path.Combine(StoragePath, "GridStorageTracker.cfg");
            _config = VeryPersistent<Config>.Load(configPath);


            LogWriter.InitializeLogger(logFolder, _config.Data.EnableDedicatedLogFile);

            _patchManager = torch.Managers.GetManager<PatchManager>();

            var ctx = _patchManager.AcquireContext();
            ctx.GetPattern(EventPatches.StoreGridMethod).Prefixes.Add(EventPatches.StoreGridPrefix);
            ctx.GetPattern(EventPatches.RetrieveGridMethod).Prefixes.Add(EventPatches.RetrieveGridPrefix);
        }
        
        public UserControl GetControl()
        {
            var grid = new PropertyGrid();
            grid.DataContext = _config.Data;
            return grid;
        }

        public override void Update()
        {
            // Ensure that the patches are committed only once during the first update cycle.
            base.Update();
            if (!_patched)
            {
                _patched = true;
                _patchManager.Commit();
            }
        }
    }
}