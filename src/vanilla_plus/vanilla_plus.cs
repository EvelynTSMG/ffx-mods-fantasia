using Fahrenheit.Core;

namespace EvelynTSMG.Mods.Fantasia;

public class VanillaPlusPatches {
    // Add utility patches here: public readonly PatchType patch_name = new();

    public void hook() {
        // Add utility patches here: patch_name.hook();
    }

    public FhSettingsCategory get_settings() {
        return new("utility", [
            // Add utility patches here: patch_name.get_settings();
        ]);
    }
}
