using Fahrenheit.Core;

namespace EvelynTSMG.Mods.Fantasia;

public abstract class FantasiaPatch {
    public abstract void hook();
    public abstract FhSettingsCategory get_settings();
}

public class Patches {
    public readonly BalancePatches balance = new();
    public readonly ExperimentalPatches experimental = new();
    public readonly MechanicsPatches mechanics = new();
    public readonly MinorMechanicsPatches minor_mechanics = new();
    public readonly UtilityPatches utility = new();
    public readonly VanillaPlusPatches vanilla_plus = new();
}
