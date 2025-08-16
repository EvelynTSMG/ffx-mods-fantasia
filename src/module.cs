using System.IO;

using Fahrenheit.Core;
using Fahrenheit.Core.FFX;
using Fahrenheit.Core.FFX.Battle;
using Fahrenheit.Core.FFX.Ids;

using Hexa.NET.ImGui;

namespace EvelynTSMG.Mods.Fantasia;

[FhLoad(FhGameType.FFX)]
public unsafe class FantasiaModule : FhModule {
    public Patches patches;
    internal FhModContext _context;
    internal FileStream _global_state;

    internal static FhLogger logger;

    public override bool init(FhModContext mod_context, FileStream global_state_file) {
        _context = mod_context;
        _global_state = global_state_file;

        logger = _logger;

        patches = new();

        settings = new FhSettingsCategory("fantasia", [
            patches.balance.get_settings(),
            patches.vanilla_plus.get_settings(),
            patches.utility.get_settings(),
            patches.minor_mechanics.get_settings(),
            patches.mechanics.get_settings(),
            patches.experimental.get_settings(),
        ]);

        patches.balance.hook();

        return true;
    }
}
