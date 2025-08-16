using Fahrenheit.Core;

namespace EvelynTSMG.Mods.Fantasia;

public class ExperimentalPatches {
    // Add experimental patches here: public readonly PatchType patch_name = new();

    public void hook() {
        // Add experimental patches here: patch_name.hook();
    }

    public FhSettingsCategory get_settings() {
        return new("experimental", [
            // Add experimental patches here: patch_name.get_settings();
        ]);
    }
}
