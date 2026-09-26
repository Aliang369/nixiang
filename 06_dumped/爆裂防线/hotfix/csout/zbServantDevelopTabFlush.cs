using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantDevelopTabFlush
{
	private zbServantWnd m_wndParent;

	private VisualElement m_RootElement;

	private zbServantSubSkill[] m_subSkills = new zbServantSubSkill[6];

	private Button[] m_btnSubSkillLocks = (Button[])(object)new Button[6];

	private Label[] m_lbCosts = (Label[])(object)new Label[2];

	private zbItem[] m_items = new zbItem[2];

	private Button m_btnFlush;

	private List<int> m_lockSubSkillList = new List<int>();

	private Sprite m_lockBg;

	private Sprite m_unLockBg;

	private string m_localId;

	public void OnEnable(zbServantWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_btnFlush = UQueryExtensions.Q<Button>(m_RootElement, "btn_flush", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_lbCosts[i] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + i, (string)null);
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		m_lockBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/servant/btn_flushLock.png");
		m_unLockBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/servant/btn_flushUnLock.png");
		for (int j = 0; j < 6; j++)
		{
			m_subSkills[j] = UQueryExtensions.Q<zbServantSubSkill>(m_RootElement, "subSkill" + j, (string)null);
			m_subSkills[j].OnEnable(m_wndParent);
			m_btnSubSkillLocks[j] = UQueryExtensions.Q<Button>(m_RootElement, "btn_lock" + j, (string)null);
			int tempIndex = j;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSubSkillLocks[j], (Action<ClickEvent>)delegate
			{
				OnLock(tempIndex);
			}, "", (TrickleDown)0);
		}
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFlush, (Action)OnFlush, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		if (m_wndParent.m_detail != null)
		{
			if (m_localId != m_wndParent.m_detail.ServantId)
			{
				m_lockSubSkillList.Clear();
			}
			m_localId = m_wndParent.m_detail.ServantId;
			RefreshSubSkills();
			RefreshFlushCost();
			RefreshLockState();
		}
	}

	private void RefreshLockState()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 6; i++)
		{
			((VisualElement)m_btnSubSkillLocks[i]).style.backgroundImage = new StyleBackground(m_unLockBg);
		}
	}

	private void RefreshSubSkills()
	{
		for (int i = 0; i < 6; i++)
		{
			bool flag = i < m_wndParent.m_detail.SubSkillList.Count;
			m_subSkills[i].Refresh(flag ? m_wndParent.m_detail.SubSkillList[i] : null);
		}
	}

	private void RefreshFlushCost()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		JArray val = (JArray)((JArray)zbSelf.Instance.servantHandler.servantBase[(object)"subSkill"][(object)"flushCost"])[m_lockSubSkillList.Count][(object)"costList"];
		for (int i = 0; i < 2; i++)
		{
			bool flag = i < ((JContainer)val).Count;
			((VisualElement)m_lbCosts[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				JToken val2 = val[i];
				zbItemData data = new zbItemData((EResourceType)Extensions.Value<int>((IEnumerable<JToken>)val2[(object)0]), Extensions.Value<int>((IEnumerable<JToken>)val2[(object)1]), Extensions.Value<int>((IEnumerable<JToken>)val2[(object)2]));
				m_items[i].Refresh(data, isNameVis: false, isCountVis: false);
				((TextElement)m_lbCosts[i]).enableRichText = true;
				((TextElement)m_lbCosts[i]).text = zbUtil.GetNeedHaveRichString(zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)Extensions.Value<int>((IEnumerable<JToken>)val2[(object)0]), Extensions.Value<int>((IEnumerable<JToken>)val2[(object)1])), Extensions.Value<int>((IEnumerable<JToken>)val2[(object)2]));
			}
		}
	}

	private void OnFlush()
	{
		if (m_wndParent.m_detail != null)
		{
			zbWsMsgHandler.Instance.Servant_SubSkillFlush_Req(m_wndParent.m_detail.ServantId, m_lockSubSkillList.ToArray());
		}
	}

	private void OnLock(int index)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if (m_wndParent.m_detail != null && index < m_wndParent.m_detail.SubSkillList.Count)
		{
			int skillId = m_wndParent.m_detail.SubSkillList[index].SkillId;
			bool flag = m_lockSubSkillList.Contains(skillId);
			if (flag)
			{
				m_lockSubSkillList.Remove(skillId);
			}
			else
			{
				m_lockSubSkillList.Add(skillId);
			}
			((VisualElement)m_btnSubSkillLocks[index]).style.backgroundImage = new StyleBackground(flag ? m_unLockBg : m_lockBg);
			RefreshFlushCost();
		}
	}
}
