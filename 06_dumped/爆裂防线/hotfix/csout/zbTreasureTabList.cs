using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTreasureTabList
{
	private zbTreasureWnd m_wndParent;

	private VisualElement m_rootElement;

	private ScrollView m_scrollView;

	private VisualElement m_btnAttr;

	private VisualElement m_nodeAttr;

	private Label m_lbAttr;

	private VisualElement m_entry;

	private Button m_btnUpgrade;

	private VisualElement m_notify;

	private int columnCount = 3;

	private List<zbTreasureTitleItem> m_listTitleItem = new List<zbTreasureTitleItem>();

	private List<zbTreasureItem> m_listItem = new List<zbTreasureItem>();

	public void OnEnable(zbTreasureWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_rootElement = root;
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_rootElement, "scrollView", (string)null);
		m_btnAttr = UQueryExtensions.Q<VisualElement>(m_rootElement, "btn_attr", (string)null);
		m_nodeAttr = UQueryExtensions.Q<VisualElement>(m_rootElement, "node_attr", (string)null);
		m_lbAttr = UQueryExtensions.Q<Label>(m_nodeAttr, "lb_attr", (string)null);
		m_entry = UQueryExtensions.Q<VisualElement>(m_nodeAttr, "entry", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_rootElement, "btn_upgrade", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>(m_btnAttr, (Action)OnAttr, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
		InitRecords();
	}

	public void Refresh()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		m_nodeAttr.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		foreach (zbTreasureTitleItem item in m_listTitleItem)
		{
			item.Refresh();
		}
		foreach (zbTreasureItem item2 in m_listItem)
		{
			item2.Refresh();
		}
		if (zbSelf.Instance.treasureHandler.HasAnyTreasureActiveNotify())
		{
			((TextElement)m_btnUpgrade).text = zbUtil.Language("lang.10946");
			((VisualElement)m_btnUpgrade).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_280_88_green.png"));
		}
		else
		{
			((TextElement)m_btnUpgrade).text = zbUtil.Language("lang.10947");
			if (zbSelf.Instance.treasureHandler.HasAnyTreasureUpgradeNotify())
			{
				((VisualElement)m_btnUpgrade).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_280_88_yellow.png"));
			}
			else
			{
				((VisualElement)m_btnUpgrade).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btnGray_280x88.png"));
			}
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.treasureHandler.HasTreasureNofity()));
	}

	private void InitRecords()
	{
		foreach (KeyValuePair<int, zbTreasureCsvRecord[]> dictionaryRecord in zbSelf.Instance.treasureHandler.dictionaryRecords)
		{
			CreateTreasureQualityRecords(dictionaryRecord.Key, dictionaryRecord.Value);
		}
	}

	private void CreateTreasureQualityRecords(int quality, zbTreasureCsvRecord[] records)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		if (records.Length == 0)
		{
			return;
		}
		VisualElement val = new VisualElement();
		val.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/treasure/treasure_s9_bg.png"));
		val.style.marginTop = StyleLength.op_Implicit(40f);
		((VisualElement)m_scrollView).Add(val);
		VisualElement val2 = new VisualElement();
		val2.style.marginTop = StyleLength.op_Implicit(-30f);
		val.Add(val2);
		zbTreasureTitleItem zbTreasureTitleItem2 = new zbTreasureTitleItem(quality);
		val2.Add((VisualElement)(object)zbTreasureTitleItem2);
		m_listTitleItem.Add(zbTreasureTitleItem2);
		VisualElement val3 = new VisualElement();
		val2.Add(val3);
		int num = Mathf.CeilToInt((float)records.Length / (float)columnCount);
		for (int i = 0; i < num; i++)
		{
			VisualElement val4 = new VisualElement();
			val4.style.flexDirection = new StyleEnum<FlexDirection>((FlexDirection)2);
			val3.Add(val4);
			for (int j = 0; j < columnCount && i * columnCount + j < records.Length; j++)
			{
				VisualElement val5 = new VisualElement();
				zbTreasureItem zbTreasureItem2 = new zbTreasureItem(m_wndParent);
				m_listItem.Add(zbTreasureItem2);
				zbTreasureItem2.Init(records[i * columnCount + j]);
				val5.Add((VisualElement)(object)zbTreasureItem2);
				val4.Add(val5);
			}
		}
	}

	private void OnAttr()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		m_nodeAttr.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_wndParent.m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_entry.Clear();
		bool flag = zbServerData.sGameData.TreasureInfo.AttrList.Count > 0;
		((TextElement)m_lbAttr).text = zbUtil.Language(flag ? "lang.10914" : "lang.10915");
		foreach (BattleTeamAttrDetail attr in zbServerData.sGameData.TreasureInfo.AttrList)
		{
			_ = attr.Value;
			Label val = new Label();
			zbTeamAttrCsvRecord recordByKey = zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey((int)attr.Type);
			((TextElement)val).text = zbAttrHelper.GetTeamAttrName((int)attr.Type) + zbAttrHelper.FormatAttrValue(attr.Value, recordByKey.showPercent == 1, isDisplayAdd: true);
			((VisualElement)val).style.fontSize = StyleLength.op_Implicit(18f);
			((VisualElement)val).style.unityFontStyleAndWeight = StyleEnum<FontStyle>.op_Implicit((FontStyle)1);
			zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)val);
			m_entry.Add((VisualElement)(object)val);
		}
	}

	public void OnTouch()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (m_wndParent.m_currTab != 1)
		{
			m_nodeAttr.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_wndParent.m_btnTouch).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void OnUpgrade()
	{
		if (zbSelf.Instance.treasureHandler.HasAnyTreasureActiveNotify())
		{
			zbWsMsgHandler.Instance.Treasure_ActiveAll_Req();
		}
		else if (zbSelf.Instance.treasureHandler.HasAnyTreasureUpgradeNotify())
		{
			zbWsMsgHandler.Instance.Treasure_UpgradeLevelAll_Req();
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10953"));
		}
	}
}
