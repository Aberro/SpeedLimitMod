using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using JetBrains.Annotations;
using Reinforced.Typings.Attributes;
using SpeedLimitEditor.Systems;
[assembly: TsGlobal(UseModules = true, ExportPureTypings = false, RootNamespace = "CommonTypes")]

namespace SpeedLimitEditor;

[UsedImplicitly]
public class Mod : IMod
{
	public static readonly ILog Log = LogManager.GetLogger($"{nameof(SpeedLimitEditor)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
	//public static Setting? Setting;
	public void OnLoad(UpdateSystem updateSystem)
	{
		Log.Info(nameof(OnLoad));

		if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
			Log.Info($"Current mod asset at {asset.path}");

		//Setting = new Setting(this);
		//Setting.RegisterInOptionsUI();
		//GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(_Setting));

		//Setting.ReloadSettings();
		//AssetDatabase.global.LoadSettings(nameof(SpeedLimitEditor), _Setting, new Setting(this));

		//updateSystem.UpdateBefore<SpeedLimitEditorUISystem>(SystemUpdatePhase.GameSimulation);
		updateSystem.UpdateAt<SpeedLimitEditorUISystem>(SystemUpdatePhase.UIUpdate);
		updateSystem.UpdateAt<EntitySelectorToolSystem>(SystemUpdatePhase.ToolUpdate);
		updateSystem.UpdateAt<SetCustomSpeedLimitsSystem>(SystemUpdatePhase.Modification4B);
		//updateSystem.UpdateBefore<UpdateRoadSignsSystem>(SystemUpdatePhase.MainLoop);
	}

	public void OnDispose()
	{
		Log.Info(nameof(OnDispose));
		//if (Setting != null)
		//{
		//    Setting.UnregisterInOptionsUI();
		//    Setting = null;
		//}
	}
}
