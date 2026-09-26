using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaSubjectZhanLingItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbItem>
	{
	}

	private zbMechaSubjectWnd m_wndParent;

	private VisualElement m_progressVal;

	private VisualElement m_grayMask;

	private VisualElement m_iconLevel;

	private Label m_lbLevel;

	private VisualElement m_bg;

	private zbItem[] m_items = new zbItem[3];

	private Sprite mainQuestIcon_Cur;

	private Sprite mainQuestIcon_Pre;

	private Sprite mainQuestIcon_Next;

	private zbMechaSubjectZhanLingCsvRecord m_record;

	public zbMechaSubjectZhanLingItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/mechaSubject/tb_mechaSubjectZhanLing.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbMechaSubjectWnd wndParent)
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

	public void Refresh(zbMechaSubjectZhanLingCsvRecord record)
	{
		m_record = record;
		((TextElement)m_lbLevel).text = m_record.count.ToString();
		bool flag = m_wndParent.m_data.ZhanLingInfo.Reach >= m_record.count;
		SetMaskVisible(!flag);
		InitAwards();
	}

	private void InitAwards()
	{
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		ZhanLingCommonInfo zhanLingInfo = m_wndParent.m_data.ZhanLingInfo;
		_ = m_wndParent.m_data.ZhanLingInfo.GoodsId;
		bool flag = m_record.count > zhanLingInfo.Reach;
		bool flag2 = zhanLingInfo != null && zhanLingInfo.Active == EBoolState.True;
		bool flag3 = m_record.count <= zhanLingInfo.LowAward;
		bool flag4 = flag2 && m_record.count <= zhanLingInfo?.HighAward;
		int num = 0;
		foreach (zbMechaSubjectZhanLingCsvRecord zhanlingRecord in zbSelf.Instance.mechaSubjectHandler.m_zhanlingRecords)
		{
			if (zhanLingInfo.Reach >= zhanlingRecord.count)
			{
				num = zhanlingRecord.count;
				continue;
			}
			break;
		}
		bool flag5 = num == m_record.count;
		bool canReceiveLowAward = !flag3 && !flag;
		bool canReceiveHighAward = (!flag4 && !flag) & flag2;
		bool flag6 = m_record.isGrand > 0;
		zbSelf.Instance.mechaSubjectHandler.SetVisualElementSprite(m_bg, flag6 ? "mechaSubject_zhanLingCell1.jpg" : "mechaSubject_zhanLingCell0.jpg");
		m_progressVal.style.height = new StyleLength(Length.Percent((float)((!flag) ? (flag5 ? 50 : 100) : 0)));
		m_iconLevel.style.backgroundImage = new StyleBackground(flag ? mainQuestIcon_Next : mainQuestIcon_Pre);
		for (int i = 0; i < 3; i++)
		{
			bool flag7 = i == 0;
			if (flag7)
			{
				bool flag8 = m_record.lowAward.Length != 0;
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag8));
				if (flag8)
				{
					int[] lowAward = m_record.lowAward[i];
					zbItemData data = new zbItemData((EResourceType)lowAward[0], lowAward[1], lowAward[2]);
					m_items[i].Refresh(data);
					m_items[i].SetGettedVisible(flag3);
					m_items[i].SetSpineVisible(canReceiveLowAward);
					m_items[i].SetTouchCallback(delegate
					{
						if (canReceiveLowAward)
						{
							zbWsMsgHandler.Instance.Operate_MechaSubjectZhanLingAward_Req(m_wndParent.m_data.BaseInfo.UUID);
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
				int num2 = i - m_record.lowAward.Length;
				bool flag9 = (m_record.highAward.Length != 0 && m_record.highAward.Length > num2) | flag6;
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag9));
				if (flag9)
				{
					int[] highAward;
					if (num2 == 0)
					{
						highAward = m_record.highAward[num2];
					}
					else
					{
						highAward = m_wndParent.mechaSubjectRecord.zhanLingAward[m_record.isGrand - 1];
					}
					zbItemData data2 = new zbItemData((EResourceType)highAward[0], highAward[1], highAward[2]);
					m_items[i].Refresh(data2);
					m_items[i].SetGettedVisible(!flag7 & flag4);
					m_items[i].SetSpineVisible(canReceiveHighAward);
					m_items[i].SetTouchCallback(delegate
					{
						if (canReceiveHighAward)
						{
							zbWsMsgHandler.Instance.Operate_MechaSubjectZhanLingAward_Req(m_wndParent.m_data.BaseInfo.UUID);
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
