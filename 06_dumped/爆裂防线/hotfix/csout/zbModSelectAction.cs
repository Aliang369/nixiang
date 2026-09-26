using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbModSelectAction : zbBaseWnd
{
	[NonSerialized]
	private VisualElement[] select_icon = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] mod_icon = (VisualElement[])(object)new VisualElement[5];

	private VisualElement m_gunObject;

	private Label m_lbGunLevel;

	private Label m_lbGunAmmo;

	private zbRectProgress m_gunProgress;

	private VisualElement[] m_nodeDetail = (VisualElement[])(object)new VisualElement[5];

	private TemplateContainer[] tb_skillMod = (TemplateContainer[])(object)new TemplateContainer[5];

	private zbPieProgress[] m_skillOutProgress = new zbPieProgress[5];

	private zbPieProgress[] m_skillInProgress = new zbPieProgress[5];

	private VisualElement[] m_skillIcon = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbSkillLevel = (Label[])(object)new Label[5];

	private float leftTime;

	private bool isActioning;

	private List<float> positionX = new List<float> { -245f, 0f, 245f };

	private List<float> positionY = new List<float> { -635f, -570f, -500f, -430f, -360f, -290f };

	private int[] targetIndex = new int[5];

	protected override void OnEnable()
	{
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_gunObject = UQueryExtensions.Q<VisualElement>(m_RootElement, "skillGun", (string)null);
		m_lbGunLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_gun", (string)null);
		m_lbGunAmmo = UQueryExtensions.Q<Label>(m_RootElement, "lb_ammo", (string)null);
		m_gunProgress = UQueryExtensions.Q<zbRectProgress>(m_RootElement, (string)null, (string)null);
		((TextElement)m_lbGunAmmo).text = "";
		m_gunProgress.progress = 0f;
		for (int i = 0; i < 5; i++)
		{
			tb_skillMod[i] = UQueryExtensions.Q<TemplateContainer>(m_RootElement, "skill" + (i + 1), (string)null);
			m_nodeDetail[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillMod[i], "nodeDetail", (string)null);
			m_skillOutProgress[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillMod[i], "outProgress", (string)null);
			m_skillOutProgress[i].progress = 100f;
			m_skillInProgress[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillMod[i], "inProgress", (string)null);
			m_skillInProgress[i].progress = 100f;
			m_skillIcon[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillMod[i], "skillIcon", (string)null);
			m_lbSkillLevel[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tb_skillMod[i], "lb_skill", (string)null);
			select_icon[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectIcon" + i, (string)null);
			select_icon[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			mod_icon[i] = UQueryExtensions.Q<VisualElement>(select_icon[i], "modIcon" + i, (string)null);
		}
	}

	public void RefreshCustom(List<int> modIdList, List<int> selectIndexList)
	{
		int num = 0;
		foreach (int modId in modIdList)
		{
			RefreshDetail(modId, selectIndexList[num]);
			num++;
		}
	}

	public void RefreshDetail(int modId, int selectIndex)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		if (selectIndex > 2)
		{
			selectIndex = 2;
		}
		((VisualElement)tb_skillMod[4]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		zbCombatData combatData = zbCombatSystem.Instance.combatData;
		List<zbHero> heroList = combatData.masterTeam.heroList;
		zbModCsvRecord modRecord = zbDataConfig.Instance.modCsvTable.GetRecordByKey(modId);
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(modRecord.icon);
		BattleSkillDetail battleSkillDetail = ((IEnumerable<BattleSkillDetail>)combatData.masterTeam.battleTeamInfo.SkillList).First((BattleSkillDetail s) => s.SkillType == (ESkillType)modRecord.skillType[0]);
		for (int num = 0; num < 5; num++)
		{
			if (!(select_icon[num].style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1)))
			{
				continue;
			}
			select_icon[num].transform.position = new Vector3(positionX[selectIndex], 60f, 0f);
			select_icon[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			mod_icon[num].style.backgroundImage = new StyleBackground(val);
			zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(battleSkillDetail.SkillId);
			Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
			select_icon[num].style.backgroundImage = new StyleBackground(val2);
			int num2 = 0;
			bool flag = false;
			bool flag2 = false;
			foreach (zbHero hero in combatData.masterTeam.heroList)
			{
				if (hero.skillDetail.SkillType == ESkillType.Qiang)
				{
					flag2 = true;
				}
				else
				{
					num2++;
				}
				if (hero.skillDetail.SkillType == (ESkillType)modRecord.skillType[0])
				{
					targetIndex[num] = num2;
					flag = true;
				}
			}
			if (1 == modRecord.skillType[0])
			{
				targetIndex[num] = 0;
			}
			else if (!flag)
			{
				targetIndex[num] = (flag2 ? heroList.Count : (heroList.Count + 1));
			}
			float vector2Distance = zbUtil.GetVector2Distance(select_icon[num].transform.position, Vector2.op_Implicit(new Vector2(360f, positionY[targetIndex[num]])));
			leftTime += vector2Distance / 1200f;
			((MonoBehaviour)this).StartCoroutine(ActionUnit(num, select_icon[num].transform.position.x, select_icon[num].transform.position.y));
			break;
		}
		refreshSkill();
		RefreshCoroutine();
		isActioning = true;
		zbPlayerGunCsvRecord recordByKey2 = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(zbSelf.Instance.GetGunId());
		m_gunObject.style.backgroundImage = StyleBackground.op_Implicit(zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey2.battleBanner));
	}

	public void RefreshCoroutine()
	{
		if (!isActioning)
		{
			((MonoBehaviour)this).StartCoroutine(ActionCoroutine());
		}
	}

	public void refreshSkill()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		zbCombatData combatData = zbCombatSystem.Instance.combatData;
		List<zbModCsvRecord> source = (from r in combatData.masterTeam.modMgr.modList
			select zbDataConfig.Instance.modCsvTable.GetRecordByKey(r.modRecord.id) into m
			where m != null
			select m).ToList();
		List<zbHero> heroList = combatData.masterTeam.heroList;
		for (int num = 0; num < 5; num++)
		{
			m_nodeDetail[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		bool flag = false;
		int num2 = 0;
		foreach (zbHero item in heroList)
		{
			if (item.skillDetail.SkillType == ESkillType.Qiang)
			{
				flag = true;
				continue;
			}
			((VisualElement)tb_skillMod[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_nodeDetail[num2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(item.skillDetail.SkillId);
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
			m_skillIcon[num2].style.backgroundImage = StyleBackground.op_Implicit(val);
			m_skillIcon[num2].transform.scale = Vector3.one * 0.45f;
			ESkillType type = item.skillDetail.SkillType;
			int num3 = source.Count((zbModCsvRecord mod) => mod.skillLvUp == 1 && Enumerable.Contains(mod.skillType, (int)type));
			((TextElement)m_lbSkillLevel[num2]).text = num3.ToString();
			num2++;
		}
		m_gunObject.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((TextElement)m_lbGunLevel).text = "Lv." + source.Count((zbModCsvRecord mod) => mod.skillLvUp == 1 && Enumerable.Contains(mod.skillType, 1));
	}

	private IEnumerator ActionCoroutine()
	{
		while (true)
		{
			bool flag = false;
			for (int i = 0; i < 5; i++)
			{
				if (select_icon[i].style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
			yield return null;
		}
		isActioning = false;
		Hide();
	}

	private IEnumerator ActionUnit(int index, float srcX, float srcY)
	{
		float tempTime = 0f;
		while (tempTime <= 1f)
		{
			tempTime += Time.unscaledDeltaTime;
			float num = Mathf.SmoothStep(srcX, 300f, tempTime / 1f);
			float num2 = Mathf.SmoothStep(srcY, positionY[targetIndex[index]], tempTime / 1f);
			select_icon[index].transform.position = new Vector3(num, num2, 0f);
			yield return null;
		}
		select_icon[index].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	protected override void UpdateAtShow()
	{
	}
}
