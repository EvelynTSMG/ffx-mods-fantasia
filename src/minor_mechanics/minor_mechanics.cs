using Fahrenheit.Core;

namespace EvelynTSMG.Mods.Fantasia;

public class MinorMechanicsPatches {
    // Add minor mechanics patches here: public readonly PatchType patch_name = new();

    public void hook() {
        // Add minor mechanics patches here: patch_name.hook();
    }

    public FhSettingsCategory get_settings() {
        return new("minor_mechanics", [
            // Add minor mechanics patches here: patch_name.get_settings();
        ]);
    }
}
