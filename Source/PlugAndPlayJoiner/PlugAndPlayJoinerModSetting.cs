using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PlugAndPlayJoiner;

internal class PlugAndPlayJoinerModSetting : ModSettings
{
    public bool autoPriorityForProfessionalWork;

    public string defaultAreaRestriction;

    private Dictionary<string, int> priorityByWorkTypeDefName;

    private Dictionary<string, int> professionalWorkMinSkills;

    private Dictionary<string, int> professionalWorkPriorities;

    public string DefaultAreaRestriction
    {
        get => defaultAreaRestriction;
        set => defaultAreaRestriction = value;
    }

    public Dictionary<string, int> PriorityByWorkTypeDefName
    {
        get
        {
            if (priorityByWorkTypeDefName != null)
            {
                return priorityByWorkTypeDefName;
            }

            priorityByWorkTypeDefName = new Dictionary<string, int>();
            foreach (var item in DefDatabase<WorkTypeDef>.AllDefs.Where(x => x.alwaysStartActive && x.visible))
            {
                priorityByWorkTypeDefName.Add(item.labelShort, 3);
            }

            return priorityByWorkTypeDefName;
        }
        set => priorityByWorkTypeDefName = value;
    }

    public Dictionary<string, int> ProfessionalWorkPriorities
    {
        get
        {
            if (professionalWorkPriorities != null)
            {
                return professionalWorkPriorities;
            }

            professionalWorkPriorities = new Dictionary<string, int>();
            foreach (var item in DefDatabase<WorkTypeDef>.AllDefs.Where(x => !x.alwaysStartActive && x.visible))
            {
                professionalWorkPriorities.Add(item.labelShort, 3);
            }

            return professionalWorkPriorities;
        }
        set => professionalWorkPriorities = value;
    }

    public Dictionary<string, int> ProfessionalWorkMinSkills
    {
        get
        {
            if (professionalWorkMinSkills != null)
            {
                return professionalWorkMinSkills;
            }

            professionalWorkMinSkills = new Dictionary<string, int>();
            foreach (var item in DefDatabase<WorkTypeDef>.AllDefs.Where(x => !x.alwaysStartActive && x.visible))
            {
                professionalWorkMinSkills.Add(item.labelShort, 8);
            }

            return professionalWorkMinSkills;
        }
        set => professionalWorkMinSkills = value;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref defaultAreaRestriction, "defaultAreaRestriction");
        Scribe_Collections.Look(ref priorityByWorkTypeDefName, "defaultWorkPriorityLookup", LookMode.Value,
            LookMode.Value);
        Scribe_Values.Look(ref autoPriorityForProfessionalWork, "autoPriorityForProfessionalWork");
        Scribe_Collections.Look(ref professionalWorkPriorities, "professionalWorkPriority", LookMode.Value,
            LookMode.Value);
        Scribe_Collections.Look(ref professionalWorkMinSkills, "professionalWorkSkillMinimun", LookMode.Value,
            LookMode.Value);
        base.ExposeData();
    }

    public void ResetAllSettings()
    {
        DefaultAreaRestriction = null;
        priorityByWorkTypeDefName = null;
        professionalWorkMinSkills = null;
        professionalWorkPriorities = null;
    }
}