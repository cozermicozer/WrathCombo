using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System;
using System.Linq;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;
namespace WrathCombo.Extensions;

internal static class BattleCharaExtensions
{
    extension(IBattleChara chara)
    {
        public uint RealHP => (uint)Math.Clamp(chara.CurrentHp + chara.PendingHPChange, 0, chara.MaxHp);
        public int PendingHPChange => CalculatePendingHPChange(chara);
        public unsafe Span<ActionEffectHandler.EffectEntry> IncomingEffects => chara.Struct()->ActionEffectHandler.IncomingEffects;

        private int CalculatePendingHPChange()
        {
            var realHp = 0;
            var effects = chara.IncomingEffects;
            foreach (var eff in effects)
            {
                if (eff.GlobalSequence == 0)
                    continue;

                foreach (var e in eff.Effects.Effects)
                {
                    var t = (ActionEffectType)e.Type;
                    if (t is ActionEffectType.Heal)
                    {
                        realHp += e.Value;
                    }
                    if (t is ActionEffectType.Damage)
                    {
                        realHp -= e.Value;
                    }
                }
            }

            return realHp;
        }
    }
    public unsafe static CombatRole GetRole(this WrathPartyMember chara)
    {
        if (chara.RealJob?.Role == 1) return CombatRole.Tank;
        if (chara.RealJob?.Role == 2) return CombatRole.DPS;
        if (chara.RealJob?.Role == 3) return CombatRole.DPS;
        if (chara.RealJob?.Role == 4) return CombatRole.Healer;
        return CombatRole.NonCombat;
    }
    public unsafe static uint RawShieldValue(this IBattleChara chara)
    {
        FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* baseVal = (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)chara.Address;
        var value = baseVal->Character.CharacterData.ShieldValue;
        var rawValue = chara.MaxHp / 100 * value;

        return rawValue;
    }

    public unsafe static byte ShieldPercentage(this IBattleChara chara)
    {
        FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* baseVal = (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)chara.Address;
        var value = baseVal->Character.CharacterData.ShieldValue;

        return value;
    }

    public static bool HasShield(this IBattleChara chara) => chara.RawShieldValue() > 0;

    public static string GetInitials(this IBattleChara chara)
    {
        var ret = string.Concat(chara.Name.TextValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.Length >= 1 && char.IsLetter(x[0]))
            .Select(x => char.ToUpper(x[0])));

        return ret;
    }
}