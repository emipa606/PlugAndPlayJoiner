using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PlugAndPlayJoiner;

[HarmonyPatch(typeof(Pawn_WorkSettings), nameof(Pawn_WorkSettings.EnableAndInitialize))]
internal class Pawn_WorkSettings_EnableAndInitialize
{
    [HarmonyPriority(350)]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        var codes = new List<CodeInstruction>(instructions);
        var getAlterProirityMethod = typeof(Pawn_WorkSettings_EnableAndInitialize).GetMethod(nameof(GetAlterPriority));
        var setPriorityMI = typeof(Pawn_WorkSettings).GetMethod(nameof(Pawn_WorkSettings.SetPriority));
        var pawnFI = typeof(Pawn_WorkSettings).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
        if (!ModLister.AnyFromListActive(["fluffy.worktab", "arof.fluffy.worktab", "arof.fluffy.worktab.continued"]))
        {
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_3 && codes[i - 1].opcode == OpCodes.Ldloc_S &&
                    ((LocalBuilder)codes[i - 1].operand).LocalIndex == 4)
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnFI);
                    yield return new CodeInstruction(OpCodes.Call, getAlterProirityMethod);
                }
                else if (codes[i].opcode == OpCodes.Ldc_I4_3 && codes[i - 1].opcode == OpCodes.Ldloc_3)
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, pawnFI);
                    yield return new CodeInstruction(OpCodes.Call, getAlterProirityMethod);
                }
                else
                {
                    yield return codes[i];
                }
            }

            if (!found)
            {
                Log.Error("[Plug and Play Joiner] transpiler failed");
            }

            yield break;
        }

        Log.Message("[Plug and Play Joiner] trying to patch for Work Tab compatibility");
        for (var i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Call && codes[i + 1].opcode == OpCodes.Ldloc_2)
            {
                found = true;
                yield return codes[i];
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, pawnFI);
                yield return new CodeInstruction(OpCodes.Call, getAlterProirityMethod);
                yield return new CodeInstruction(OpCodes.Call, setPriorityMI);
            }
            else if (codes[i].opcode == OpCodes.Call && codes[i + 1].opcode == OpCodes.Ldloc_0)
            {
                found = true;
                yield return codes[i];
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldloc_3);
                yield return new CodeInstruction(OpCodes.Ldloc_3);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, pawnFI);
                yield return new CodeInstruction(OpCodes.Call, getAlterProirityMethod);
                yield return new CodeInstruction(OpCodes.Call, setPriorityMI);
            }
            else
            {
                yield return codes[i];
            }
        }

        if (!found)
        {
            Log.Error("[Plug and Play Joiner] transpiler failed when patching work tab.");
        }
        else
        {
            Log.Message("[Plug and Play Joiner] Work Tab gui-compatibility patched");
        }
    }

    public static int GetAlterPriority(WorkTypeDef worktype, Pawn pawn)
    {
        var raceProps = pawn.RaceProps;
        if (raceProps is { IsMechanoid: true })
        {
            return 3;
        }

        if (PlugAndPlayJoinerModHandler.Settings.PriorityByWorkTypeDefName.TryGetValue(worktype.labelShort,
                out var priority))
        {
            return priority;
        }

        try
        {
            if (PlugAndPlayJoinerModHandler.Settings.autoPriorityForProfessionalWork &&
                PlugAndPlayJoinerModHandler.Settings.ProfessionalWorkPriorities.ContainsKey(worktype.labelShort) &&
                PlugAndPlayJoinerModHandler.Settings.ProfessionalWorkMinSkills.ContainsKey(worktype.labelShort) &&
                (pawn.skills.AverageOfRelevantSkillsFor(worktype) >=
                 PlugAndPlayJoinerModHandler.Settings.ProfessionalWorkMinSkills[worktype.labelShort] ||
                 pawn.skills.MaxPassionOfRelevantSkillsFor(worktype) != 0))
            {
                return PlugAndPlayJoinerModHandler.Settings.ProfessionalWorkPriorities[worktype.labelShort];
            }
        }
        catch
        {
            return 3;
        }

        if (worktype.alwaysStartActive || !PlugAndPlayJoinerModHandler.Settings.autoPriorityForProfessionalWork)
        {
            return 3;
        }

        return 0;
    }
}