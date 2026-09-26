using System.Collections.Generic;
using Proto.Enums;

public class zbCombatDefine
{
	public static string ANIMATION_SEQUENCE_IDLE = "idle";

	public static string ANIMATION_SEQUENCE_RUN = "run";

	public static string ANIMATION_SEQUENCE_SHOW = "show";

	public static string ANIMATION_SEQUENCE_PHYATTACK = "attack";

	public static string ANIMATION_SEQUENCE_RELOAD = "reload";

	public static string ANIMATION_SEQUENCE_ACTION = "action";

	public static string DUMMY_BEHIT = "dummy_behit";

	public static string DUMMY_ROOT = "root";

	public static string DUMMY_GUNPOINT = "gunpoint";

	public static string COMBAT_PREFABFOLDER = "prefab/combat/hero/";

	public static List<ESkillType> needAdjustHeroRotation = new List<ESkillType>
	{
		ESkillType.Qiang,
		ESkillType.Bingdan,
		ESkillType.Jiguang,
		ESkillType.Pao,
		ESkillType.Ranshaodan,
		ESkillType.Shexian
	};

	public static List<ESkillType> mainSkillNeedKnockback = new List<ESkillType>
	{
		ESkillType.Qiang,
		ESkillType.Pao,
		ESkillType.Bingdan,
		ESkillType.Hongzha,
		ESkillType.Motuo,
		ESkillType.Fengren
	};

	public static List<ESkillType> mainSkillNeedTow = new List<ESkillType> { ESkillType.Longjuan };
}
