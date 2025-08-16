using System;
using System.Runtime.InteropServices;

using Fahrenheit.Core;
using Fahrenheit.Core.FFX;
using Fahrenheit.Core.FFX.Battle;

namespace EvelynTSMG.Mods.Fantasia.Balance;

public unsafe class ElementalAffinitiesRebalanced : FantasiaPatch {
    public enum BalanceType {
        DEFAULT,     // For compatibility. Defer to the original function. If no other mods affect elemental affinity calculations, behaves like Favorable.
        FAVORABLE,   // Vanilla. When resolving multi-elemental strikes, favors the player. Can stack x1.5s.
        BALANCED,    // Default. When resolving N-elemental strikes, treats each 1/Nth of the damage as a separate element.
        UNFAVORABLE, // When resolving multi-elemental strikes, favors the enemy. Can stack x1/2s.
        EXTRA_MEAN,  // When resolving multi-elemental strikes, favors the enemy. Can stack x1/2s. Can stack x1.5s if absorbing.
    }

    public static class Settings {
        public static readonly FhSettingDropdown<BalanceType> balance_type =
                new("type", BalanceType.BALANCED);

        public static readonly FhSettingsCategoryToggleable is_enabled =
                new("elemental_affinities", [
                    balance_type,
                ]);
    }

    /// <summary>
    /// Calculates elemental multipliers on the damage.
    /// </summary>
    /// <param name="target">The target of the damage calculation.</param>
    /// <param name="command">The command that is dealing the damage.</param>
    /// <param name="elements">Which elements the attack is exhibiting. A mix of weapon and command elements.</param>
    /// <param name="damage">The damage calculated so far.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int DmgCalc_Elem(Chr* target, PCommand* command, ElementFlags elements, int damage);

    private FhMethodHandle<DmgCalc_Elem> _h_DmgCalc_Elem;

    public override void hook() {
        Settings.is_enabled.set(true);

        _h_DmgCalc_Elem = new(null, "FFX.exe", h_DmgCalc_Elem, offset: 0x38a420);
        _h_DmgCalc_Elem.hook();
    }

    public override FhSettingsCategory get_settings() => Settings.is_enabled;

    private int h_DmgCalc_Elem(Chr* target, PCommand* command, ElementFlags elements, int damage) {
        if (!Settings.is_enabled.get())
            return _h_DmgCalc_Elem.orig_fptr(target,command, elements, damage);

        if (elements == ElementFlags.NONE) return damage;

        return Settings.balance_type.get() switch {
            BalanceType.DEFAULT     => _h_DmgCalc_Elem.orig_fptr(target, command, elements, damage),
            BalanceType.FAVORABLE   => DmgCalc_Elem_Favorable   (target, command, elements, damage),
            BalanceType.BALANCED    => DmgCalc_Elem_Balanced    (target, command, elements, damage),
            BalanceType.UNFAVORABLE => DmgCalc_Elem_Unfavorable (target, command, elements, damage),
            BalanceType.EXTRA_MEAN  => DmgCalc_Elem_ExtraMean   (target, command, elements, damage),
            _ => throw new NotImplementedException($"Unknown Elemental Affinities Rebalancing Type: {Settings.balance_type.get()}"),
        };
    }

    private static int DmgCalc_Elem_Balanced(Chr* target, PCommand* command, ElementFlags elements, int damage) {
        double new_damage = 0;

        int element_count = 0;
        if (elements.HasFlag(ElementFlags.FIRE)) element_count++;
        if (elements.HasFlag(ElementFlags.ICE)) element_count++;
        if (elements.HasFlag(ElementFlags.THUNDER)) element_count++;
        if (elements.HasFlag(ElementFlags.WATER)) element_count++;
        if (elements.HasFlag(ElementFlags.HOLY)) element_count++;

        double damage_part = (double)damage / element_count;

        foreach (ElementFlags element in Enum.GetValues<ElementFlags>()) {
            if (element == ElementFlags.NONE) continue; // Whoops
            if (!elements.HasFlag(element)) continue;

            if (target->elem_weak.HasFlag(element)) {
                new_damage += damage_part * 1.5;
                continue;
            }

            if (target->elem_resist.HasFlag(element)) {
                new_damage += damage_part * 0.5;
                continue;
            }

            if (target->elem_ignore.HasFlag(element)) continue;

            if (target->elem_absorb.HasFlag(element)) {
                new_damage += damage_part * -1.0;
                continue;
            }

            new_damage += damage_part;
        }

        return (int)Math.Round(new_damage);
    }

    private static int DmgCalc_Elem_Favorable(Chr* target, PCommand* command, ElementFlags elements, int damage) {
        double new_damage = (double)damage;

        new_damage = calc_damage(target->elem_weak, elements, new_damage, 1.5, true, out bool weak_proc);
        if (weak_proc) return (int)Math.Round(new_damage);

        foreach (ElementFlags element in Enum.GetValues<ElementFlags>()) {
            if (!elements.HasFlag(element)) continue;

            // Weakness has already been accounted for
            if (target->elem_resist.HasFlag(element)) continue;
            if (target->elem_ignore.HasFlag(element)) continue;
            if (target->elem_absorb.HasFlag(element)) continue;

            return damage;
        }

        new_damage = calc_damage(target->elem_resist, elements, new_damage, 0.5, false, out bool resist_proc);
        if (resist_proc) return (int)Math.Round(new_damage);

        new_damage = calc_damage(target->elem_ignore, elements, new_damage, 0.0, false, out bool ignore_proc);
        if (ignore_proc) return 0;

        new_damage = calc_damage(target->elem_absorb, elements, new_damage, -1.0, false, out _);
        return (int)Math.Round(new_damage);
    }

    private static int DmgCalc_Elem_Unfavorable(Chr* target, PCommand* command, ElementFlags elements, int damage) {
        double new_damage = (double)damage;
        new_damage = calc_damage(target->elem_absorb, elements, new_damage, -1.0, false, out bool absorbed);
        if (absorbed) return -damage;

        new_damage = calc_damage(target->elem_ignore, elements, new_damage, 0.0, false, out bool ignored);
        if (ignored) return 0;

        new_damage = calc_damage(target->elem_resist, elements, new_damage, 0.5, true, out bool resisted);
        if (resisted) return (int)Math.Round(new_damage);

        new_damage = calc_damage(target->elem_weak, elements, new_damage, 1.5, false, out _);
        return (int)Math.Round(new_damage);
    }

    private static int DmgCalc_Elem_ExtraMean(Chr* target, PCommand* command, ElementFlags elements, int damage) {
        double new_damage = (double)damage;
        new_damage = calc_damage(target->elem_absorb, elements, new_damage, -1.0, false, out bool absorbed);
        if (absorbed) {
            new_damage = calc_damage(target->elem_weak, elements, new_damage, 1.5, true, out _);
            return (int)Math.Round(new_damage);
        }

        new_damage = calc_damage(target->elem_ignore, elements, new_damage, 0.0, false, out bool ignored);
        if (ignored) return 0;

        new_damage = calc_damage(target->elem_resist, elements, new_damage, 0.5, true, out bool resisted);
        if (resisted) return (int)Math.Round(new_damage);

        new_damage = calc_damage(target->elem_weak, elements, new_damage, 1.5, false, out _);
        return (int)Math.Round(new_damage);
    }

    private static double calc_damage(ElementFlags affinity, ElementFlags elements, double damage, double mult, bool stacking, out bool applied) {
        applied = false;

        foreach (ElementFlags element in Enum.GetValues<ElementFlags>()) {
            if (element == ElementFlags.NONE) continue; // Whoops
            if (!affinity.HasFlag(element)) continue;
            if (!elements.HasFlag(element)) continue;

            applied = true;

            if (stacking) damage *= mult;
            else return damage * mult;
        }

        return damage;
    }
}
