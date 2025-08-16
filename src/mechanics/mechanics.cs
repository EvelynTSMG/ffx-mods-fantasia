using Fahrenheit.Core;

namespace EvelynTSMG.Mods.Fantasia;

public class MechanicsPatches {
    // Add mechanics patches here: public readonly PatchType patch_name = new();

    public void hook() {
        // Add mechanics patches here: patch_name.hook();
    }

    public FhSettingsCategory get_settings() {
        return new("mechanics", [
            // Add mechanics patches here: patch_name.get_settings();
        ]);
    }
}
