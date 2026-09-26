using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHolidayTaskAwardItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbHolidayTaskAwardItem>
	{
	}

	private zbHolidayTaskWnd m_wndParent;

	private VisualElement m_progressVal;

	private VisualElement m_grayMask;

	private VisualElement m_iconLevel;

	private Label m_lbLevel;

	private VisualElement m_bg;

	private zbItem[] m_items = new zbItem[3];

	public Sprite mainQuestIcon_Cur;

	public Sprite mainQuestIcon_Pre;

	public Sprite mainQuestIcon_Next;

	private zbHolidayTaskLevelCsvRecord m_record;

	public zbHolidayTaskAwardItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_holidayAwardtem.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbHolidayTaskWnd wndParent)
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
		m_grayMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "gray_mask", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbHolidayTaskLevelCsvRecord record)
	{
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		bool flag = m_record.level > m_wndParent.m_data.Level;
		bool flag2 = m_wndParent.m_data.Level == m_record.level;
		bool flag3 = m_wndParent.m_data.ActiveState == 1;
		bool flag4 = m_record.level <= m_wndParent.m_data.LowAwardLevel;
		bool flag5 = flag3 && m_record.level <= m_wndParent.m_data.HighAwardLevel;
		bool canReceiveLowAward = !flag4 && !flag;
		bool canReceiveHighAward = (!flag5 && !flag) & flag3;
		((TextElement)m_lbLevel).text = "Lv." + m_record.level;
		SetMaskVisible(flag);
		m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>((m_record.actAward > 0) ? "ui/image/legendary/legendary_zhanling_cell2.png" : "ui/image/legendary/legendary_zhanling_cell1.png"));
		m_progressVal.style.height = new StyleLength(Length.Percent((float)((!flag) ? ((flag2 && m_record.actAward == 0) ? 50 : 100) : 0)));
		m_iconLevel.style.backgroundImage = new StyleBackground(flag ? mainQuestIcon_Next : mainQuestIcon_Pre);
		for (int i = 0; i < 3; i++)
		{
			bool flag6 = i == 0;
			int[][] awardList = (flag6 ? m_record.awardLow : m_record.awardHigh);
			if (flag6)
			{
				int[][] array = awardList;
				foreach (int[] award in array)
				{
					zbItemData data = new zbItemData((EResourceType)award[0], award[1], award[2]);
					m_items[i].Refresh(data);
					m_items[i].SetGettedVisible(flag4);
					m_items[i].SetSpineVisible(canReceiveLowAward);
					m_items[i].SetTouchCallback(delegate
					{
						if (canReceiveLowAward)
						{
							zbWsMsgHandler.Instance.Operate_HolidayTaskLevelAward_Req(m_wndParent.m_data.BaseInfo.UUID, m_record.level);
						}
						else
						{
							zbUIMgr.Instance.ShowTip((EResourceType)award[0], award[1], 0L);
						}
					});
				}
			}
			else
			{
				int curIndex = i - m_record.awardLow.Length;
				bool flag7 = curIndex < m_record.awardHigh.Length;
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag7));
				if (flag7)
				{
					zbItemData data2 = new zbItemData((EResourceType)awardList[curIndex][0], awardList[curIndex][1], awardList[curIndex][2]);
					m_items[i].Refresh(data2);
					m_items[i].SetGettedVisible(flag5);
					m_items[i].SetSpineVisible(canReceiveHighAward);
					m_items[i].SetTouchCallback(delegate
					{
						if (canReceiveHighAward)
						{
							zbWsMsgHandler.Instance.Operate_HolidayTaskLevelAward_Req(m_wndParent.m_data.BaseInfo.UUID, m_record.level);
						}
						else
						{
							zbUIMgr.Instance.ShowTip((EResourceType)awardList[curIndex][0], awardList[curIndex][1], 0L);
						}
					});
				}
			}
			((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_items[i], "icon_lock", (string)null)).visible = ((i == 0) ? flag : (flag || !flag3));
		}
	}

	public void SetMaskVisible(bool visible)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_grayMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
	}
}
