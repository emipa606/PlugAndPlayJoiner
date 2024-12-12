using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mlie;
using RimWorld;
using UnityEngine;
using Verse;

namespace PlugAndPlayJoiner;

[UsedImplicitly]
internal class PlugAndPlayJoinerModHandler : Mod
{
    public static PlugAndPlayJoinerModSetting Settings;

    private static string currentVersion;

    private readonly Area_NoRoof areaNoRoof = new Area_NoRoof();

    private readonly Area_BuildRoof areaRoof = new Area_BuildRoof();

    private readonly Area_SnowClear areaSnowClear = new Area_SnowClear();

    private Vector2 scrollPos = new Vector2(0f, 0f);

    public PlugAndPlayJoinerModHandler(ModContentPack content)
        : base(content)
    {
        Settings = GetSettings<PlugAndPlayJoinerModSetting>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override void DoSettingsWindowContents(Rect rect)
    {
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect);
        if (Current.Game == null)
        {
            listing_Standard.Label("SettingAreaRestrictionWarning".Translate());
        }
        else
        {
            if (listing_Standard.ButtonTextLabeled("DefaultAreaRestriction".Translate(),
                    Settings.DefaultAreaRestriction))
            {
                var list = new List<FloatMenuOption>();
                foreach (var area in Find.CurrentMap.areaManager.AllAreas.Where(x =>
                             x.Label != areaRoof.Label && x.Label != areaNoRoof.Label &&
                             x.Label != areaSnowClear.Label))
                {
                    list.Add(
                        new FloatMenuOption(area.Label, delegate { Settings.DefaultAreaRestriction = area.Label; }));
                }

                list.Add(new FloatMenuOption("NoAreaAllowed".Translate(),
                    delegate { Settings.DefaultAreaRestriction = null; }));
                Find.WindowStack.Add(new FloatMenu(list));
            }
        }

        listing_Standard.Gap(5f);
        listing_Standard.GapLine(2f);
        listing_Standard.Gap(5f);
        listing_Standard.Label("DefaultWorkPriority".Translate());
        listing_Standard.Gap(5f);
        listing_Standard.CheckboxLabeled("AutoProfessionalWorkPriority".Translate(),
            ref Settings.autoPriorityForProfessionalWork);
        listing_Standard.Gap(5f);
        if (currentVersion != null)
        {
            GUI.contentColor = Color.gray;
            listing_Standard.Label("PnP.CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
            listing_Standard.Gap(5f);
        }

        listing_Standard.End();
        var listing_Standard2 = new Listing_Standard();
        var outRect = new Rect(rect.x, rect.y + listing_Standard.CurHeight, rect.width,
            rect.height - listing_Standard.CurHeight);
        var rect2 = new Rect(rect.x, rect.y, rect.width - 20f, Settings.ProfessionalWorkPriorities.Count * 40f);
        Widgets.BeginScrollView(outRect, ref scrollPos, rect2);
        listing_Standard2.Begin(rect2);
        listing_Standard2.ColumnWidth = (rect2.width - 34f) / 3f;
        foreach (var key2 in Settings.PriorityByWorkTypeDefName.Keys.ToList())
        {
            if (listing_Standard2.ButtonTextLabeled(key2, Settings.PriorityByWorkTypeDefName[key2].ToString()))
            {
                try
                {
                    var list2 = new List<FloatMenuOption>();
                    for (var i = 0; i <= 4; i++)
                    {
                        var intBuf2 = i.ToString();
                        list2.Add(new FloatMenuOption(intBuf2,
                            delegate { Settings.PriorityByWorkTypeDefName[key2] = int.Parse(intBuf2); }));
                    }

                    Find.WindowStack.Add(new FloatMenu(list2));
                }
                catch
                {
                    Log.Error("Error when trying to set work priority.");
                }
            }

            listing_Standard2.Gap(3f);
        }

        listing_Standard2.Gap(30f);
        if (listing_Standard2.ButtonText("ResetAllButton".Translate()))
        {
            Settings.ResetAllSettings();
        }

        listing_Standard2.NewColumn();
        listing_Standard2.Label("ProfessionalWorkPriority".Translate());
        foreach (var key in Settings.ProfessionalWorkPriorities.Keys.ToList())
        {
            if (!listing_Standard2.ButtonTextLabeled(key, Settings.ProfessionalWorkPriorities[key].ToString()))
            {
                continue;
            }

            try
            {
                var list3 = new List<FloatMenuOption>();
                for (var j = 0; j <= 4; j++)
                {
                    var intBuf = j.ToString();
                    list3.Add(new FloatMenuOption(intBuf,
                        delegate { Settings.ProfessionalWorkPriorities[key] = int.Parse(intBuf); }));
                }

                Find.WindowStack.Add(new FloatMenu(list3));
            }
            catch
            {
                Log.Error("Error when trying to set professional work priority.");
            }
        }

        listing_Standard2.NewColumn();
        listing_Standard2.Label("MinimiumAverageSkills".Translate());
        foreach (var item in Settings.ProfessionalWorkMinSkills.Keys.ToList())
        {
            var i2 = Settings.ProfessionalWorkMinSkills[item];
            var buffer = i2.ToString();
            var intContainer = new IntContainer(i2);
            Widgets.TextFieldNumericLabeled(listing_Standard2.GetRect(30f), item, ref intContainer.index, ref buffer,
                0f, 20f);
            Settings.ProfessionalWorkMinSkills[item] = intContainer.index;
            listing_Standard2.Gap(2f);
        }

        listing_Standard2.End();
        Widgets.EndScrollView();
        base.DoSettingsWindowContents(rect);
    }

    public override string SettingsCategory()
    {
        return "PlugAndPlayJoiner".Translate();
    }
}