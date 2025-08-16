using EvelynTSMG.Mods.Fantasia.Balance;

using Fahrenheit.Core;

namespace EvelynTSMG.Mods.Fantasia;

public class BalancePatches {
    ElementalAffinitiesRebalanced elemental_affinities = new();
    // Add balance patches here: public readonly PatchType patch_name = new();

    public void hook() {
        elemental_affinities.hook();
        // Add balance patches here: patch_name.hook();
    }

    public FhSettingsCategory get_settings() {
        return new("balance", [
            elemental_affinities.get_settings(),
            // Add balance patches here: patch_name.get_settings();
        ]);
    }
}
