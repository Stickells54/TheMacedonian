using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Library;
using TheMacedonian.Behaviors;

namespace TheMacedonian
{
    /// <summary>
    /// The Macedonian: Rise of the Usurper
    /// Entry point for the mod. Registers behaviors, applies Harmony patches.
    /// </summary>
    public class SubModule : MBSubModuleBase
    {
        private Harmony? _harmony;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            
            _harmony = new Harmony("com.macedonian.usurper");
            _harmony.PatchAll();
            
            InformationManager.DisplayMessage(new InformationMessage(
                "[The Macedonian] Mod loaded. The path to power awaits.", 
                Colors.Cyan));
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
            _harmony?.UnpatchAll("com.macedonian.usurper");
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            
            if (game.GameType is Campaign && gameStarterObject is CampaignGameStarter campaignStarter)
            {
                // Register the core behavior
                campaignStarter.AddBehavior(new MacedonianBehavior());
                
                InformationManager.DisplayMessage(new InformationMessage(
                    "[The Macedonian] Campaign behaviors registered.", 
                    Colors.Green));
            }
        }
    }
}
