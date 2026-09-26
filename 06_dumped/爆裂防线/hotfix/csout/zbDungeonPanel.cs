using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbDungeonPanel
{
	private zbDungeonWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label[] m_lbRiotProgress = (Label[])(object)new Label[10];

	private VisualElement m_hpProVal;

	private Label m_lbHpProVal;

	private Label m_lbName;

	private VisualElement[] m_skillIcon = (VisualElement[])(object)new VisualElement[4];

	private VisualElement m_gun;

	private VisualElement m_riotBubblePnl;

	private Label m_lblRiotbubble;

	private Coroutine m_riotBubbleCoroutine;

	private Sprite m_riotGray;

	private Sprite m_riotRed;

	private DungeonBaseInfo m_dungeonBaseInfo;

	public void OnEnable(zbDungeonWnd wndParent)
	{
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		VisualElement rootElement = m_wndParent.GetRootElement();
		for (int i = 0; i < 10; i++)
		{
			m_lbRiotProgress[i] = UQueryExtensions.Q<Label>(rootElement, "icon_warn" + i, (string)null);
		}
		m_hpProVal = UQueryExtensions.Q<VisualElement>(rootElement, "hpProVal", (string)null);
		m_lbHpProVal = UQueryExtensions.Q<Label>(rootElement, "lb_hpProVal", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(rootElement, "lblname", (string)null);
		m_gun = UQueryExtensions.Q<VisualElement>(rootElement, "gun", (string)null);
		m_riotGray = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/dungeon/icon_processGray.png");
		m_riotRed = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/dungeon/icon_processRed.png");
		VisualElement val = UQueryExtensions.Q<VisualElement>(rootElement, "skillinfo", (string)null);
		for (int j = 0; j < 4; j++)
		{
			int tempIndex = j + 1;
			m_skillIcon[j] = UQueryExtensions.Q<VisualElement>(val, "skill" + tempIndex, (string)null);
			wndParent.m_uiEvent.RegisterCallback<ClickEvent>(m_skillIcon[j], (Action)delegate
			{
				OnSkillClick(tempIndex);
			}, "", (TrickleDown)0);
		}
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>(m_gun, (Action)delegate
		{
			OnSkillClick(0);
		}, "", (TrickleDown)0);
		m_riotBubblePnl = UQueryExtensions.Q<VisualElement>(rootElement, "riotbubble", (string)null);
		m_lblRiotbubble = UQueryExtensions.Q<Label>(m_riotBubblePnl, "lbl_riotbubble", (string)null);
		m_riotBubblePnl.style.opacity = StyleFloat.op_Implicit(0f);
	}

	public void Refresh(DungeonBaseInfo dungeonBaseInfo)
	{
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		if (dungeonBaseInfo == null)
		{
			return;
		}
		int num = 0;
		if (m_dungeonBaseInfo != null && dungeonBaseInfo != null)
		{
			num = dungeonBaseInfo.RiotValue - m_dungeonBaseInfo.RiotValue;
		}
		else if (m_dungeonBaseInfo == null && dungeonBaseInfo != null)
		{
			num = dungeonBaseInfo.RiotValue;
		}
		m_dungeonBaseInfo = dungeonBaseInfo;
		zbDungeonCsvRecord recordByKey = zbDataConfig.Instance.dungeonCsvTable.GetRecordByKey(dungeonBaseInfo.DungeonId);
		if (recordByKey == null)
		{
			return;
		}
		((TextElement)m_lbName).text = recordByKey.name;
		for (int i = 0; i < m_lbRiotProgress.Length; i++)
		{
			bool flag = i < dungeonBaseInfo.RiotValue;
			((VisualElement)m_lbRiotProgress[i]).style.backgroundImage = new StyleBackground(flag ? m_riotRed : m_riotGray);
		}
		if (num > 0)
		{
			VisualElement val = (VisualElement)(object)m_lbRiotProgress[dungeonBaseInfo.RiotValue - 1];
			if (val != null)
			{
				zbUIEffect.BlinkTimes(val);
			}
			((TextElement)m_lblRiotbubble).text = "+" + num;
			m_riotBubblePnl.style.opacity = StyleFloat.op_Implicit(1f);
			float num2 = 790f;
			m_riotBubblePnl.style.bottom = StyleLength.op_Implicit(num2);
			if (m_riotBubbleCoroutine != null)
			{
				((MonoBehaviour)m_wndParent).StopCoroutine(m_riotBubbleCoroutine);
			}
			m_riotBubbleCoroutine = ((MonoBehaviour)m_wndParent).StartCoroutine(PlayRiotBubbleAnim(num2));
		}
		m_hpProVal.style.width = new StyleLength(new Length(dungeonBaseInfo.DefensePct, (LengthUnit)1));
		((TextElement)m_lbHpProVal).text = $"{Mathf.Floor(dungeonBaseInfo.DefensePct)}%";
		refreshSkill();
	}

	private IEnumerator PlayRiotBubbleAnim(float startBottom)
	{
		yield return null;
		float endBottom = startBottom + 50f;
		float duration = 1f;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			float num2 = Mathf.Lerp(startBottom, endBottom, num);
			float num3 = Mathf.Lerp(1f, 0f, num);
			m_riotBubblePnl.style.bottom = StyleLength.op_Implicit(num2);
			m_riotBubblePnl.style.opacity = StyleFloat.op_Implicit(num3);
			yield return null;
		}
	}

	public void refreshSkill()
	{
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		List<zbModCsvRecord> source = (from r in (IEnumerable<int>)m_dungeonBaseInfo.ModList
			select zbDataConfig.Instance.modCsvTable.GetRecordByKey(r) into m
			where m != null
			select m).ToList();
		for (int num = 0; num < 4; num++)
		{
			m_skillIcon[num].visible = false;
		}
		for (int num2 = 0; num2 < 4; num2++)
		{
			int tmpIndex = num2 + 1;
			if (tmpIndex < m_dungeonBaseInfo.SkillList.Count)
			{
				m_skillIcon[num2].visible = true;
				zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(m_dungeonBaseInfo.SkillList[tmpIndex].SkillId);
				int num3 = source.Count((zbModCsvRecord mod) => mod.skillLvUp == 1 && Enumerable.Contains(mod.skillType, (int)m_dungeonBaseInfo.SkillList[tmpIndex].SkillType));
				Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
				m_skillIcon[num2].style.backgroundImage = StyleBackground.op_Implicit(val);
				((TextElement)UQueryExtensions.Q<Label>(m_skillIcon[num2], "lb_skill", (string)null)).text = num3.ToString();
			}
		}
		((TextElement)UQueryExtensions.Q<Label>(m_gun, "lb_skill", (string)null)).text = "Lv." + source.Count((zbModCsvRecord mod) => mod.skillLvUp == 1 && Enumerable.Contains(mod.skillType, 1));
	}

	private void OnSkillClick(int index)
	{
		zbUIMgr.Instance.ShowSkillFightModWnd(index, ((IEnumerable<SkillDetail>)m_dungeonBaseInfo.SkillList).ToArray(), ((IEnumerable<int>)m_dungeonBaseInfo.ModList).ToList(), isResumeFight: false);
	}
}
