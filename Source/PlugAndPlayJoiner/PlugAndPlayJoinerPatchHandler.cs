using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace PlugAndPlayJoiner;

[StaticConstructorOnStartup]
internal class PlugAndPlayJoinerPatchHandler
{
    public static int LowestPrio = 4;
    private static readonly FieldInfo WorkTabMaxPrioFieldInfo;
    private static readonly object PriorityMasterSettingObject;
    private static readonly MethodInfo PriorityMasterMaxPrioMethodInfo;
    public static readonly bool WorkTabEnabled;
    public static readonly bool PriorityMasterEnabled;

    static PlugAndPlayJoinerPatchHandler()
    {
        if (ModLister.AnyFromListActive(["fluffy.worktab", "arof.fluffy.worktab", "arof.fluffy.worktab.continued"]))
        {
            Log.Message("[Plug and Play Joiner] trying to find maxPriority-setting from WorkTab");
            WorkTabMaxPrioFieldInfo = AccessTools.Field(AccessTools.TypeByName("WorkTab.Settings"), "maxPriority");
            if (WorkTabMaxPrioFieldInfo != null)
            {
                WorkTabEnabled = true;
                Log.Message("[Plug and Play Joiner] WorkTab-compatibility implemented");
            }
            else
            {
                Log.Message(
                    "[Plug and Play Joiner] failed to find the maxPriority-field from the WorkTab mod, will not be able to auto-set priorities using it");
            }
        }

        if (ModLister.AnyFromListActive(["lauriichan.prioritymaster"]))
        {
            Log.Message("[Plug and Play Joiner] trying to find settings-field from PriorityMaster");
            var priorityMasterSettingFieldInfo =
                AccessTools.Field(AccessTools.TypeByName("PriorityMod.Core.PriorityMaster"), "settings");
            if (priorityMasterSettingFieldInfo != null)
            {
                Log.Message("[Plug and Play Joiner] trying to find GetMaxPriority-method from PriorityMaster");
                PriorityMasterMaxPrioMethodInfo =
                    AccessTools.Method(AccessTools.TypeByName("PriorityMod.Settings.PrioritySettings"),
                        "GetMaxPriority");

                if (PriorityMasterMaxPrioMethodInfo != null)
                {
                    PriorityMasterSettingObject = priorityMasterSettingFieldInfo.GetValue(null);
                    PriorityMasterEnabled = true;
                    Log.Message("[Plug and Play Joiner] PriorityMaster-compatibility implemented");
                }
                else
                {
                    Log.Message(
                        "[Plug and Play Joiner] failed to find the GetMaxPriority-method from the PriorityMaster mod, will not be able to auto-set priorities using it");
                }
            }
            else
            {
                Log.Message(
                    "[Plug and Play Joiner] failed to find the settings-field from the PriorityMaster mod, will not be able to auto-set priorities using it");
            }
        }

        RefreshLowestPrio();
        new Harmony("Telardo.PlugAndPlayJoiner").PatchAll(Assembly.GetExecutingAssembly());
    }

    public static void RefreshLowestPrio()
    {
        if (WorkTabEnabled)
        {
            var beforeValue = LowestPrio;
            LowestPrio = (int)WorkTabMaxPrioFieldInfo.GetValue(null);
            if (LowestPrio != beforeValue)
            {
                Log.Message($"[Plug and Play Joiner] updating the lowest possible prio to {LowestPrio} from WorkTab");
            }
        }

        if (PriorityMasterEnabled)
        {
            var beforeValue = LowestPrio;
            LowestPrio = (int)PriorityMasterMaxPrioMethodInfo.Invoke(PriorityMasterSettingObject, null);
            if (LowestPrio != beforeValue)
            {
                Log.Message(
                    $"[Plug and Play Joiner] updating the lowest possible prio to {LowestPrio} from PriorityMaster");
            }
        }

        if (PlugAndPlayJoinerModHandler.Settings.PriorityByWorkTypeDefName.Any())
        {
            for (var i = 0; i < PlugAndPlayJoinerModHandler.Settings.PriorityByWorkTypeDefName.Count; i++)
            {
                var priority = PlugAndPlayJoinerModHandler.Settings.PriorityByWorkTypeDefName.ElementAt(i);

                if (priority.Value > LowestPrio)
                {
                    PlugAndPlayJoinerModHandler.Settings.PriorityByWorkTypeDefName[priority.Key] = LowestPrio;
                }
            }
        }

        if (!PlugAndPlayJoinerModHandler.Settings.ProfessionalWorkPriorities.Any())
        {
            return;
        }

        for (var i = 0; i < PlugAndPlayJoinerModHandler.Settings.ProfessionalWorkPriorities.Count; i++)
        {
            var priority = PlugAndPlayJoinerModHandler.Settings.ProfessionalWorkPriorities.ElementAt(i);

            if (priority.Value > LowestPrio)
            {
                PlugAndPlayJoinerModHandler.Settings.ProfessionalWorkPriorities[priority.Key] = LowestPrio;
            }
        }
    }
}