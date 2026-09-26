using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMainQuestItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbMainQuestItem>
	{
	}

	private VisualElement m_progressVal;

	private VisualElement m_grayMask;

	private VisualElement m_iconDay;

	private Label m_lbDay;

	private zbBaseWnd m_wndParent;

	private VisualElement m_bg;

	private zbItem[] m_items = new zbItem[3];

	private zbZhanLingMainCsvRecord m_data;

	private ZhanLingMainActiveDetail m_activeDetail;

	private ZhanLingMainInfo m_mainInfo;

	public Sprite mainQuestIcon_Cur;

	public Sprite mainQuestIcon_Pre;

	public Sprite mainQuestIcon_Next;

	private int m_passStage;

	public zbMainQuestItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_mainquest.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbBaseWnd wndParent)
	{
		m_wndParent = wndParent;
		m_progressVal = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progressVal", (string)null);
		m_grayMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "gray_mask", (string)null);
		m_lbDay = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_day", (string)null);
		m_iconDay = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_day", (string)null);
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

	public void Init(zbZhanLingMainCsvRecord data, ZhanLingMainActiveDetail activeDetail, int passStage)
	{
		m_passStage = passStage;
		m_data = data;
		m_activeDetail = activeDetail;
		m_mainInfo = zbServerData.sGameData.RechargeActiveInfo.ZhanLingMainInfo;
		if (m_data != null && m_mainInfo != null)
		{
			((TextElement)m_lbDay).text = m_data.mainLevel.ToString();
			bool flag = m_mainInfo.Reach >= data.mainLevel;
			SetMaskVisible(!flag);
			InitAward();
		}
	}

	private void InitAward()
	{
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		bool flag = m_data.mainLevel > m_mainInfo.Reach;
		bool flag2 = m_activeDetail.Active == EBoolState.True;
		bool flag3 = m_data.mainLevel <= m_mainInfo.LowAward;
		bool flag4 = flag2 && m_data.mainLevel <= m_activeDetail.HighAward;
		bool flag5 = zbSelf.Instance.passportHandler.IsCurrentZhanling(m_passStage, m_data.mainLevel);
		bool canReceiveLowAward = !flag3 && !flag;
		bool canReceiveHighAward = (!flag4 && !flag) & flag2;
		bool flag6 = zbSelf.Instance.passportHandler.IsGrandPrize(m_passStage, m_data.mainLevel);
		m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag6 ? "ui/image/passport/grandPrize_bg.jpg" : "ui/image/bg2/mainQuestItem_bg.png"));
		m_progressVal.style.height = new StyleLength(Length.Percent((float)((!flag) ? ((flag5 && !flag6) ? 50 : 100) : 0)));
		m_iconDay.style.backgroundImage = new StyleBackground(flag ? mainQuestIcon_Next : mainQuestIcon_Pre);
		for (int i = 0; i < 3; i++)
		{
			bool flag7 = i == 0;
			if (flag7)
			{
				bool flag8 = m_data.lowAward.Length != 0;
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag8));
				if (flag8)
				{
					int[] lowAward = m_data.lowAward[i];
					zbItemData data = new zbItemData((EResourceType)lowAward[0], lowAward[1], lowAward[2]);
					m_items[i].Refresh(data);
					m_items[i].SetGettedVisible(flag3);
					m_items[i].SetSpineVisible(canReceiveLowAward);
					m_items[i].SetTouchCallback(delegate
					{
						if (canReceiveLowAward)
						{
							ZhanLingMainInfo zhanLingMainInfo = zbServerData.sGameData.RechargeActiveInfo.ZhanLingMainInfo;
							List<zbZhanLingMainCsvRecord> zhanlingMainRecordsByStage = zbSelf.Instance.passportHandler.GetZhanlingMainRecordsByStage(m_passStage);
							zbWsMsgHandler.Instance.RechargeActive_ZhanLingMainAward_Req(Mathf.Min(zhanlingMainRecordsByStage[zhanlingMainRecordsByStage.Count - 1].mainLevel, zhanLingMainInfo.Reach));
						}
						else
						{
							zbUIMgr.Instance.ShowTip((EResourceType)lowAward[0], lowAward[1], 0L);
						}
					});
				}
			}
			else
			{
				int num = i - m_data.lowAward.Length;
				bool flag9 = m_data.highAward.Length != 0 && m_data.highAward.Length > num;
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag9));
				if (flag9)
				{
					int[] highAward = m_data.highAward[num];
					zbItemData data2 = new zbItemData((EResourceType)highAward[0], highAward[1], highAward[2]);
					m_items[i].Refresh(data2);
					m_items[i].SetGettedVisible(!flag7 & flag4);
					m_items[i].SetSpineVisible(canReceiveHighAward);
					m_items[i].SetTouchCallback(delegate
					{
						if (canReceiveHighAward)
						{
							ZhanLingMainInfo zhanLingMainInfo = zbServerData.sGameData.RechargeActiveInfo.ZhanLingMainInfo;
							List<zbZhanLingMainCsvRecord> zhanlingMainRecordsByStage = zbSelf.Instance.passportHandler.GetZhanlingMainRecordsByStage(m_passStage);
							zbWsMsgHandler.Instance.RechargeActive_ZhanLingMainAward_Req(Mathf.Min(zhanlingMainRecordsByStage[zhanlingMainRecordsByStage.Count - 1].mainLevel, zhanLingMainInfo.Reach));
						}
						else
						{
							zbUIMgr.Instance.ShowTip((EResourceType)highAward[0], highAward[1], 0L);
						}
					});
				}
			}
			((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_items[i], "icon_lock", (string)null)).visible = ((i == 0) ? flag : (flag || !flag2));
		}
	}

	public void SetMaskVisible(bool visible)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_grayMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
	}
}
