using Fahrenheit.Core;

namespace EvelynTSMG.Mods.Fantasia;

public class UtilityPatches {
    // Add vanilla+ patches here: public readonly PatchType patch_name = new();

    public void hook() {
        // Add vanilla+ patches here: patch_name.hook();
    }

    public FhSettingsCategory get_settings() {
        return new("vanilla_plus", [
            // Add vanilla+ patches here: patch_name.get_settings();
        ]);
    }
}
