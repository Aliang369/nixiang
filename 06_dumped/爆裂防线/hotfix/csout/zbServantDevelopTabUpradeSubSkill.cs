using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantDevelopTabUpradeSubSkill
{
	private zbServantWnd m_wndParent;

	private VisualElement m_RootElement;

	private zbServantSubSkill[] m_subSkills = new zbServantSubSkill[6];

	private Label[] m_lbCosts = (Label[])(object)new Label[2];

	private zbItem[] m_items = new zbItem[2];

	private Button m_btnUpgrade;

	private VisualElement m_upgradeNotify;

	public void OnEnable(zbServantWnd wndParent, VisualElement root)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_upgradeNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(0f, 0f), 1f, true);
		for (int i = 0; i < 2; i++)
		{
			m_lbCosts[i] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + i, (string)null);
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		for (int j = 0; j < 6; j++)
		{
			m_subSkills[j] = UQueryExtensions.Q<zbServantSubSkill>(m_RootElement, "subSkill" + j, (string)null);
			m_subSkills[j].OnEnable(m_wndParent);
		}
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		if (m_wndParent.m_detail != null)
		{
			RefreshSubSkills();
			RefreshCost();
			RefreshNotify();
		}
	}

	private void RefreshCost()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		for (int i = 0; i < 2; i++)
		{
			bool flag = i < ((JContainer)zbSelf.Instance.servantHandler.subSkillUpgradeCost).Count;
			((VisualElement)m_lbCosts[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				JArray val = (JArray)zbSelf.Instance.servantHandler.subSkillUpgradeCost[i];
				zbItemData data = new zbItemData((EResourceType)Extensions.Value<int>((IEnumerable<JToken>)val[0]), Extensions.Value<int>((IEnumerable<JToken>)val[1]), Extensions.Value<int>((IEnumerable<JToken>)val[2]));
				m_items[i].Refresh(data, isNameVis: false, isCountVis: false);
				((TextElement)m_lbCosts[i]).enableRichText = true;
				((TextElement)m_lbCosts[i]).text = zbUtil.GetNeedHaveRichString(zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)Extensions.Value<int>((IEnumerable<JToken>)val[0]), Extensions.Value<int>((IEnumerable<JToken>)val[1])), Extensions.Value<int>((IEnumerable<JToken>)val[2]));
			}
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

	private void RefreshNotify()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		m_upgradeNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.servantHandler.CanServantUpgradeSubSkill(m_wndParent.m_detail)));
	}

	private void OnUpgrade()
	{
		if (m_wndParent.m_detail != null)
		{
			if (((IEnumerable<ServantSkillDetail>)m_wndParent.m_detail.SubSkillList).All((ServantSkillDetail s) => s.Level == Extensions.Value<int>((IEnumerable<JToken>)zbSelf.Instance.servantHandler.servantBase[(object)"subSkill"][(object)"levelLimit"])))
			{
				zbUtil.Language("lang.11089");
			}
			else if (zbSelf.Instance.servantHandler.CanServantUpgradeSubSkill(m_wndParent.m_detail))
			{
				zbWsMsgHandler.Instance.Servant_UpgradeSubSkill_Req(m_wndParent.m_detail.ServantId);
			}
		}
	}
}
