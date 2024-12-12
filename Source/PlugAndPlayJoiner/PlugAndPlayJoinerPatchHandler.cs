using System.Reflection;
using HarmonyLib;
using Verse;

namespace PlugAndPlayJoiner;

[StaticConstructorOnStartup]
internal class PlugAndPlayJoinerPatchHandler
{
    static PlugAndPlayJoinerPatchHandler()
    {
        new Harmony("Telardo.PlugAndPlayJoiner").PatchAll(Assembly.GetExecutingAssembly());
    }
}