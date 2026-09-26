using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegendaryZhanLingItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbLegendaryZhanLingItem>
	{
	}

	private zbBaseWnd m_wndParent;

	private VisualElement m_progressVal;

	private VisualElement m_grayMask;

	private VisualElement m_iconLevel;

	private Label m_lbLevel;

	private VisualElement m_bg;

	private zbItem[] m_items = new zbItem[3];

	public Sprite mainQuestIcon_Cur;

	public Sprite mainQuestIcon_Pre;

	public Sprite mainQuestIcon_Next;

	private zbZhanLingLegendaryCsvRecord m_record;

	private int m_passStage;

	public zbLegendaryZhanLingItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/legendary/tb_legendaryZhanLing.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbBaseWnd wndParent)
	{
		m_wndParent = wndParent;
		m_progressVal = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progressVal", (string)null);
		m_grayMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "gray_mask", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_iconLevel = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_level", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		mainQuestIcon_Cur = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/passport/mainQuestItem_current.png");
		mainQuestIcon_Next = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/passport/mainQuestItem_next.png");
		mainQuestIcon_Pre = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/passport/mainQuestItem_pre.png");
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, $"item{i}", (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbZhanLingLegendaryCsvRecord record, int passStage)
	{
		m_record = record;
		m_passStage = passStage;
		if (m_record != null && zbServerData.sGameData.RechargeActiveInfo.ZhanLingLegendaryInfo != null)
		{
			((TextElement)m_lbLevel).text = m_record.lv.ToString();
			bool flag = zbServerData.sGameData.RechargeActiveInfo.ZhanLingLegendaryInfo.Reach >= m_record.lv;
			SetMaskVisible(!flag);
			RefreshDetail();
		}
	}

	private void RefreshDetail()
	{
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		ZhanLingMainActiveDetail zhanlingActiveDetailByStage = zbSelf.Instance.legendaryHandler.GetZhanlingActiveDetailByStage(m_passStage);
		bool flag = m_record.lv > zbServerData.sGameData.RechargeActiveInfo.ZhanLingLegendaryInfo.Reach;
		bool flag2 = zhanlingActiveDetailByStage != null && zhanlingActiveDetailByStage.Active == EBoolState.True;
		bool flag3 = m_record.lv <= zbServerData.sGameData.RechargeActiveInfo.ZhanLingLegendaryInfo.LowAward;
		bool flag4 = flag2 && m_record.lv <= zhanlingActiveDetailByStage?.HighAward;
		bool flag5 = zbSelf.Instance.legendaryHandler.IsCurrentZhanling(m_passStage, m_record.lv);
		bool flag6 = !flag3 && !flag;
		bool flag7 = (!flag4 && !flag) & flag2;
		bool flag8 = zbSelf.Instance.legendaryHandler.IsGrandPrize(m_passStage, m_record.lv);
		m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag8 ? "ui/image/legendary/legendary_zhanling_cell2.png" : "ui/image/legendary/legendary_zhanling_cell1.png"));
		m_progressVal.style.height = new StyleLength(Length.Percent((float)((!flag) ? ((flag5 && !flag8) ? 50 : 100) : 0)));
		m_iconLevel.style.backgroundImage = new StyleBackground(flag ? mainQuestIcon_Next : mainQuestIcon_Pre);
		for (int i = 0; i < 3; i++)
		{
			zbItem zbItem2 = m_items[i];
			bool flag9 = i == 0;
			int[] array = null;
			bool flag10 = false;
			bool gettedVisible = false;
			bool canReceive = false;
			if (flag9)
			{
				flag10 = m_record.lowAward != null && m_record.lowAward.Length != 0;
				if (flag10)
				{
					array = m_record.lowAward[0];
					gettedVisible = flag3;
					canReceive = flag6;
				}
			}
			else
			{
				int num = i - 1;
				flag10 = m_record.highAward != null && num >= 0 && num < m_record.highAward.Length;
				if (flag10)
				{
					array = m_record.highAward[num];
					gettedVisible = flag4;
					canReceive = flag7;
				}
			}
			((VisualElement)zbItem2).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag10));
			if (!flag10)
			{
				continue;
			}
			int type = array[0];
			int id = array[1];
			int num2 = array[2];
			zbItem2.Refresh(new zbItemData((EResourceType)type, id, num2));
			zbItem2.SetGettedVisible(gettedVisible);
			zbItem2.SetSpineVisible(canReceive);
			zbItem2.SetTouchCallback(delegate
			{
				if (canReceive)
				{
					ZhanLingMainInfo zhanLingLegendaryInfo = zbServerData.sGameData.RechargeActiveInfo.ZhanLingLegendaryInfo;
					List<zbZhanLingLegendaryCsvRecord> zhanlingRecordsByStage = zbSelf.Instance.legendaryHandler.GetZhanlingRecordsByStage(m_passStage);
					zbWsMsgHandler.Instance.RechargeActive_ZhanLingLegendary_Req(Mathf.Min(zhanlingRecordsByStage[zhanlingRecordsByStage.Count - 1].lv, zhanLingLegendaryInfo.Reach));
				}
				else
				{
					zbUIMgr.Instance.ShowTip((EResourceType)type, id, 0L);
				}
			});
			((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)zbItem2, "icon_lock", (string)null)).visible = (flag9 ? flag : (flag || !flag2));
		}
	}

	public void SetMaskVisible(bool visible)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_grayMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
	}
}
