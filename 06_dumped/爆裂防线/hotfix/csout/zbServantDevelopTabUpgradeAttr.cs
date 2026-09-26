using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantDevelopTabUpgradeAttr
{
	private zbServantWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbLv;

	private Label m_lbDesc;

	private Label m_lbCount;

	private VisualElement m_mainAttr;

	private Label m_mainAttrKey;

	private Label m_mainAttrValLeft;

	private VisualElement[] m_subAttrs = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_subAttrsKey = (Label[])(object)new Label[5];

	private Label[] m_subAttrsValLeft = (Label[])(object)new Label[5];

	private Button m_btnUpgrade;

	private VisualElement m_upgradeNotify;

	public void OnEnable(zbServantWnd wndParent, VisualElement root)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_lv", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_mainAttr = UQueryExtensions.Q<VisualElement>(m_RootElement, "mainAttr", (string)null);
		m_mainAttrKey = UQueryExtensions.Q<Label>(m_mainAttr, "lb_key", (string)null);
		m_mainAttrValLeft = UQueryExtensions.Q<Label>(m_mainAttr, "lb_val", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_upgradeNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(0f, 0f), 1f, true);
		for (int i = 0; i < 5; i++)
		{
			m_subAttrs[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "attr" + i, (string)null);
			m_subAttrsKey[i] = UQueryExtensions.Q<Label>(m_subAttrs[i], "lb_key", (string)null);
			m_subAttrsValLeft[i] = UQueryExtensions.Q<Label>(m_subAttrs[i], "lb_val", (string)null);
		}
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		if (m_wndParent.m_detail != null)
		{
			RefreshAttrs();
			RefreshDesc();
			RefreshNotify();
		}
	}

	private void RefreshDesc()
	{
		int num = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.servantBaseJsonTable.GetRecord()["subAttr"][(object)"levelInterval"]);
		((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.11042"), num);
		((TextElement)m_lbCount).text = string.Format(zbUtil.Language("lang.11073"), (m_wndParent.m_detail.SubAttrUpgradeCount - m_wndParent.m_detail.SubAttrUpgradeCounted).ToString());
	}

	private void RefreshAttrs()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		zbServantQualityCsvRecord recordByKey = zbDataConfig.Instance.servantQualityCsvTable.GetRecordByKey((int)m_wndParent.m_detail.Quality);
		((TextElement)m_lbLv).text = $"{m_wndParent.m_detail.Level} / {recordByKey.lvLimit} ";
		zbServantAttrCsvRecord recordByKey2 = zbDataConfig.Instance.servantAttrCsvTable.GetRecordByKey(m_wndParent.m_record.mainAttrId);
		Color val = zbUtil.ParseHexColor(zbSelf.Instance.servantHandler.GetColorByQuality(m_wndParent.m_detail.Quality));
		((VisualElement)m_mainAttrKey).style.color = StyleColor.op_Implicit(val);
		((TextElement)m_mainAttrKey).text = zbAttrHelper.GetAttrName((EAttrType)recordByKey2.attrType, recordByKey2.attr[0], recordByKey2.attr[1]);
		zbSelf.Instance.servantHandler.GetAttrValByQuality(m_wndParent.m_detail.Quality, recordByKey2);
		_ = m_wndParent.m_detail.Level;
		((TextElement)m_mainAttrValLeft).text = zbSelf.Instance.servantHandler.FormatAttrVal(m_wndParent.m_record.mainAttrId, m_wndParent.m_detail.Quality, m_wndParent.m_detail.Level);
		_ = recordByKey.lvLimit;
		_ = m_wndParent.m_detail.Level;
		_ = m_wndParent.m_detail.Quality;
		_ = zbDataConfig.Instance.servantQualityCsvTable.GetRecords().records.Count;
		zbSelf.Instance.servantHandler.GetAttrValByQuality(m_wndParent.m_detail.Quality, recordByKey2);
		_ = m_wndParent.m_detail.Level;
		for (int i = 0; i < 5; i++)
		{
			bool flag = i < m_wndParent.m_detail.SubAttrList.Count;
			m_subAttrs[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				RepeatedField<ServantAttrDetail> subAttrList = m_wndParent.m_detail.SubAttrList;
				zbServantAttrCsvRecord recordByKey3 = zbDataConfig.Instance.servantAttrCsvTable.GetRecordByKey(subAttrList[i].AttrId);
				Color val2 = zbUtil.ParseHexColor(zbSelf.Instance.servantHandler.GetColorByQuality(subAttrList[i].Quality));
				((VisualElement)m_subAttrsKey[i]).style.color = StyleColor.op_Implicit(val2);
				((TextElement)m_subAttrsKey[i]).text = zbAttrHelper.GetAttrName((EAttrType)recordByKey3.attrType, recordByKey3.attr[0], recordByKey3.attr[1]);
				zbSelf.Instance.servantHandler.GetAttrValByQuality(subAttrList[i].Quality, recordByKey3);
				_ = m_wndParent.m_detail.Level;
				((TextElement)m_subAttrsValLeft[i]).text = zbSelf.Instance.servantHandler.FormatAttrVal(subAttrList[i].AttrId, subAttrList[i].Quality, m_wndParent.m_detail.Level);
			}
		}
	}

	private void RefreshNotify()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		m_upgradeNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.servantHandler.CanServantUpgradeAttr(m_wndParent.m_detail)));
	}

	private void OnUpgrade()
	{
		if (m_wndParent.m_detail != null && zbSelf.Instance.servantHandler.CanServantUpgradeAttr(m_wndParent.m_detail))
		{
			zbWsMsgHandler.Instance.Servant_UpgradeAttr_Req(m_wndParent.m_detail.ServantId);
		}
	}
}
