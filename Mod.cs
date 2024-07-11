using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.Settings;
using SpeedLimitEditor.System;

namespace SpeedLimitEditor;

public class Mod : IMod
{
    public static ILog Log = LogManager.GetLogger($"{nameof(SpeedLimitEditor)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
    public static Setting? Setting;
    public void OnLoad(UpdateSystem updateSystem)
    {
        Log.Info(nameof(OnLoad));

        if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            Log.Info($"Current mod asset at {asset.path}");

        Setting = new Setting(this);
        Setting.RegisterInOptionsUI();
        GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(_Setting));

        //Setting.ReloadSettings();
        AssetDatabase.global.LoadSettings(nameof(SpeedLimitEditor), _Setting, new Setting(this));

        updateSystem.UpdateAt<SpeedLimitEditorUISystem>(SystemUpdatePhase.UIUpdate);
    }

    public void OnDispose()
    {
        Log.Info(nameof(OnDispose));
        if (Setting != null)
        {
            Setting.UnregisterInOptionsUI();
            Setting = null;
        }
    }
}
