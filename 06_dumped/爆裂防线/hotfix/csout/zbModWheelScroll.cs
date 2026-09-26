using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbModWheelScroll : VisualElement
{
	public VisualElement root;

	private VisualTreeAsset visualTree;

	public List<VisualElement> spriteIcons = new List<VisualElement>();

	private zbCombatTeam masterTeam;

	public zbModWheelScroll(VisualElement _root)
	{
		root = _root;
		visualTree = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_modWheelScroll.uxml");
		visualTree.CloneTree((VisualElement)(object)this);
		for (int i = 0; i < 8; i++)
		{
			spriteIcons.Add(UQueryExtensions.Q<VisualElement>(root, "sp_icon" + i, (string)null));
		}
	}

	public void RefreshDetail(zbCombatTeam _masterTeam)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		masterTeam = _masterTeam;
		List<string> list = new List<string>();
		foreach (zbHero hero in masterTeam.heroList)
		{
			zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(hero.skillDetail.SkillId);
			list.Add(recordByKey.icon);
		}
		for (int i = 0; i < 5; i++)
		{
			int index = i % list.Count;
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(list[index]);
			spriteIcons[i].style.backgroundImage = new StyleBackground(val);
		}
		for (int j = 5; j < 8; j++)
		{
			int index2 = (j - 5) % list.Count;
			Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(list[index2]);
			spriteIcons[j].style.backgroundImage = new StyleBackground(val2);
		}
	}

	public void RefreshIcon(int index, int modId)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (modId == 0)
		{
			spriteIcons[index].style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/icon/skill/skill_0.png"));
			return;
		}
		zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(modId);
		zbHero heroBySkillType = masterTeam.GetHeroBySkillType((ESkillType)recordByKey.skillType[0]);
		if (!((Object)(object)heroBySkillType == (Object)null))
		{
			zbSkillCsvRecord recordByKey2 = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(heroBySkillType.skillDetail.SkillId);
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon);
			spriteIcons[index].style.backgroundImage = new StyleBackground(val);
		}
	}
}
