using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionWarZhanlingItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbLegionWarZhanlingItem>
	{
	}

	private zbBaseWnd m_wndParent;

	private zbItem[] m_awards = new zbItem[2];

	private VisualElement m_legionWarItem;

	private VisualElement m_iconLegionWarItem;

	private Label m_lbLegionWarItem;

	private VisualElement m_iconLegionWarItemLock;

	private VisualElement m_progress;

	private Label m_lbDay;

	private VisualElement m_grayMask;

	private VisualElement m_iconDone;

	private Button m_btnHigh;

	private zbLegionWarLevelCsvRecord m_record;

	private Dictionary<int, string> imgUrl = new Dictionary<int, string>
	{
		{ 1, "ui/image/legionWar/icon_6.png" },
		{ 2, "ui/image/legionWar/icon_7.png" }
	};

	public zbLegionWarZhanlingItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/legionWar/tb_legionWarZhanling.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbBaseWnd wndParent)
	{
		m_wndParent = wndParent;
		for (int i = 0; i < 2; i++)
		{
			m_awards[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_awards[i].OnEnable(m_wndParent);
		}
		m_legionWarItem = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "legionWarItem", (string)null);
		m_iconLegionWarItem = UQueryExtensions.Q<VisualElement>(m_legionWarItem, "icon_legionwarItem", (string)null);
		m_iconLegionWarItemLock = UQueryExtensions.Q<VisualElement>(m_legionWarItem, "icon_lock", (string)null);
		m_lbLegionWarItem = UQueryExtensions.Q<Label>(m_legionWarItem, "lb_value", (string)null);
		m_btnHigh = UQueryExtensions.Q<Button>(m_legionWarItem, "btn_high", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progressVal", (string)null);
		m_lbDay = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_day", (string)null);
		m_grayMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "gray_mask", (string)null);
		m_iconDone = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_done", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHigh, (Action)OnHigh, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(zbLegionWarLevelCsvRecord record)
	{
		m_record = record;
		InitAwards();
	}

	private void InitAwards()
	{
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbDay).text = m_record.level.ToString();
		LegionWarZhanLingInfo legionWarZhanLingInfo = zbServerData.sGameData.LegionWarBaseInfo.LegionWarZhanLingInfo;
		JObject record = zbDataConfig.Instance.vipBaseJsonTable.GetRecord();
		Extensions.Value<int>((IEnumerable<JToken>)record["legionWar"][(object)"mediumGoodsId"]);
		Extensions.Value<int>((IEnumerable<JToken>)record["legionWar"][(object)"highGoodsId"]);
		bool flag = legionWarZhanLingInfo.MediumActive == EBoolState.True;
		bool flag2 = legionWarZhanLingInfo.HighActive == EBoolState.True;
		bool flag3 = legionWarZhanLingInfo.Level < m_record.level;
		bool flag4 = legionWarZhanLingInfo.Level == m_record.level;
		bool flag5 = m_record.level <= legionWarZhanLingInfo.LowAward;
		bool flag6 = flag && m_record.level <= legionWarZhanLingInfo.MediumAward;
		bool item = !flag5 && !flag3;
		bool item2 = (!flag6 && !flag3) & flag;
		(int[], bool, bool)[] array = new(int[], bool, bool)[2]
		{
			(m_record.awardLow, flag5, item),
			(m_record.awardMedium, flag6, item2)
		};
		for (int i = 0; i < array.Length && i < m_awards.Length; i++)
		{
			zbItem obj = m_awards[i];
			int[] item3 = array[i].Item1;
			bool item4 = array[i].Item2;
			bool item5 = array[i].Item3;
			zbItemData data = new zbItemData((EResourceType)item3[0], item3[1], item3[2]);
			obj.Refresh(data);
			obj.SetGettedVisible(item4);
			obj.SetSpineVisible(item5);
			bool canReceiveCopy = item5;
			int[] dataCopy = item3;
			obj.SetTouchCallback(delegate
			{
				if (canReceiveCopy)
				{
					zbWsMsgHandler.Instance.LegionWar_ZhanLingAward_Req();
				}
				else
				{
					zbUIMgr.Instance.ShowTip((EResourceType)dataCopy[0], dataCopy[1], 0L);
				}
			});
			Label obj2 = UQueryExtensions.Q<Label>((VisualElement)(object)m_awards[i], "icon_lock", (string)null);
			bool visible = false;
			switch (i)
			{
			case 0:
				visible = flag3;
				break;
			case 1:
				visible = flag3 || !flag;
				break;
			}
			((VisualElement)obj2).visible = visible;
			m_progress.style.height = new StyleLength(Length.Percent((float)((!flag3) ? (flag4 ? 50 : 100) : 0)));
			m_grayMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
		}
		m_iconLegionWarItem.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(imgUrl[m_record.awardHighType]));
		if (m_record.awardHighType == 1)
		{
			((TextElement)m_lbLegionWarItem).text = " -" + m_record.awardHigh * 100f + "%";
		}
		else
		{
			((TextElement)m_lbLegionWarItem).text = " +" + m_record.awardHigh;
		}
		m_iconDone.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(!flag3 & flag2)));
		m_iconLegionWarItemLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((!flag3 & flag2) ? 1 : 0));
	}

	private void OnHigh()
	{
		if (m_record.awardHighType == 1)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11255"), m_record.awardHigh * 100f + "%"));
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11232"), m_record.awardHigh.ToString()));
		}
	}
}
