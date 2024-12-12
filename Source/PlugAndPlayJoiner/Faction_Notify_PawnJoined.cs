using HarmonyLib;
using RimWorld;
using Verse;

namespace PlugAndPlayJoiner;

[HarmonyPatch(typeof(Faction), nameof(Faction.Notify_PawnJoined))]
public static class Faction_Notify_PawnJoined
{
    public static void Postfix(Pawn p)
    {
        SetDefaultAreaFor(p);
    }

    public static void SetDefaultAreaFor(Pawn pawn)
    {
        var playerFaction = pawn.Faction != null && pawn.Faction == Faction.OfPlayerSilentFail;
        var playerHostFaction = pawn.HostFaction != null && pawn.HostFaction == Faction.OfPlayerSilentFail;
        var isMechanoid = pawn.RaceProps?.IsMechanoid ?? false;
        var areaToSet =
            Find.CurrentMap?.areaManager?.GetLabeled(PlugAndPlayJoinerModHandler.Settings.DefaultAreaRestriction);
        if (!playerFaction && !playerHostFaction || isMechanoid ||
            pawn.playerSettings is not { AreaRestrictionInPawnCurrentMap: null } || areaToSet == null)
        {
            return;
        }

        LongEventHandler.QueueLongEvent(delegate { pawn.playerSettings.AreaRestrictionInPawnCurrentMap = areaToSet; },
            "SetArea", false, null);
    }
}