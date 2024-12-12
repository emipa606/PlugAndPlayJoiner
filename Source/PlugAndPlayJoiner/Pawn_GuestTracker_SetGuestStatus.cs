using HarmonyLib;
using RimWorld;
using Verse;

namespace PlugAndPlayJoiner;

[HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.SetGuestStatus))]
public class Pawn_GuestTracker_SetGuestStatus
{
    public static void Postfix(Pawn ___pawn)
    {
        Faction_Notify_PawnJoined.SetDefaultAreaFor(___pawn);
    }
}