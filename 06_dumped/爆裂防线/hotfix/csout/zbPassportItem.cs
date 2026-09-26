using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPassportItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbPassportItem>
	{
	}

	private Label m_lbPro;

	private VisualElement m_grayMask;

	private VisualElement m_bg;

	private VisualElement m_iconDay;

	private VisualElement m_progressVal;

	private Label m_lbDay;

	private zbItem[] m_items = new zbItem[3];

	private zbBaseWnd m_wndParent;

	private PassportInfo m_passportInfo;

	private zbPassportLevelAwardCsvRecord m_data;

	public Sprite mainQuestIcon_Pre;

	public Sprite mainQuestIcon_Next;

	public zbPassportItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/peerage/tb_passportItem.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbBaseWnd wndParent)
	{
		m_wndParent = wndParent;
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_lbPro = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_pro", (string)null);
		m_grayMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "gray_mask", (string)null);
		m_lbDay = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_day", (string)null);
		m_iconDay = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_day", (string)null);
		m_progressVal = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progressVal", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, $"item{i}", (string)null);
			m_items[i].OnEnable(m_wndParent);
		}
		mainQuestIcon_Next = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/passport/mainQuestItem_next.png");
		mainQuestIcon_Pre = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/passport/mainQuestItem_pre.png");
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(zbPassportLevelAwardCsvRecord data)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		bool flag = m_data.level == zbDataConfig.Instance.passportLevelAwardCsvTable.GetRecords().records.Count;
		m_bg.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? "ui/image/peerage/passport_bg4.jpg" : "ui/image/peerage/passport_bg3.png"));
		m_passportInfo = zbServerData.sGameData.RechargeActiveInfo.PassportInfo;
		PassportLevelInfo levelInfo = m_passportInfo.LevelInfo;
		PassportBaseInfo baseInfo = m_passportInfo.BaseInfo;
		bool flag2 = m_data.level > levelInfo.Level;
		bool flag3 = m_data.level == levelInfo.Level;
		bool flag4 = baseInfo.Active == EBoolState.True;
		bool flag5 = m_data.level <= m_passportInfo.LevelInfo.LowAward;
		bool flag6 = flag4 && m_data.level <= m_passportInfo.LevelInfo.HighAward;
		bool canReceiveLowAward = !flag5 && !flag2;
		bool canReceiveHighAward = (!flag6 && !flag2) & flag4;
		m_progressVal.style.height = new StyleLength(Length.Percent((float)((!flag2) ? ((flag3 && !flag) ? 50 : 100) : 0)));
		m_iconDay.style.backgroundImage = new StyleBackground(flag2 ? mainQuestIcon_Next : mainQuestIcon_Pre);
		m_grayMask.visible = flag2;
		((VisualElement)m_lbPro).style.height = new StyleLength(Length.Percent((float)((!flag2) ? (flag3 ? 50 : 100) : 0)));
		((TextElement)m_lbDay).text = data.level.ToString();
		for (int i = 0; i < 3; i++)
		{
			bool flag7 = i == 0;
			int[][] awardList = (flag7 ? data.lowAward : data.highAward);
			if (flag7)
			{
				int[][] array = awardList;
				foreach (int[] award in array)
				{
					zbItemData data2 = new zbItemData((EResourceType)award[0], award[1], award[2]);
					m_items[i].Refresh(data2);
					m_items[i].SetGettedVisible(flag5);
					m_items[i].SetSpineVisible(canReceiveLowAward);
					m_items[i].SetTouchCallback(delegate
					{
						if (canReceiveLowAward)
						{
							zbWsMsgHandler.Instance.Passport_LevelAward_Req(data.level);
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
				int curIndex = i - data.lowAward.Length;
				bool flag8 = curIndex < data.highAward.Length;
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag8));
				if (flag8)
				{
					zbItemData data3 = new zbItemData((EResourceType)awardList[curIndex][0], awardList[curIndex][1], awardList[curIndex][2]);
					m_items[i].Refresh(data3);
					m_items[i].SetGettedVisible(flag6);
					m_items[i].SetSpineVisible(canReceiveHighAward);
					m_items[i].SetTouchCallback(delegate
					{
						if (canReceiveHighAward)
						{
							zbWsMsgHandler.Instance.Passport_LevelAward_Req(data.level);
						}
						else
						{
							zbUIMgr.Instance.ShowTip((EResourceType)awardList[curIndex][0], awardList[curIndex][1], 0L);
						}
					});
				}
			}
			((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_items[i], "icon_lock", (string)null)).visible = ((i == 0) ? flag2 : (flag2 || !flag4));
		}
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void SetMaskVisible(bool visible)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_grayMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
	}
}
