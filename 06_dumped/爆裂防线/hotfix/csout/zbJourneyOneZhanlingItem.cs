using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbJourneyOneZhanlingItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbJourneyOneZhanlingItem>
	{
	}

	private zbBaseWnd m_wndParent;

	private zbItem[] m_awards = new zbItem[3];

	private VisualElement m_progress;

	private Label m_lbDay;

	private VisualElement m_grayMask;

	private zbChapterOneZhanLingCsvRecord m_record;

	public zbJourneyOneZhanlingItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/journey/journeyOne/tb_journeyOneZhanling.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbBaseWnd wndParent)
	{
		m_wndParent = wndParent;
		for (int i = 0; i < 3; i++)
		{
			m_awards[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, "item" + i, (string)null);
			m_awards[i].OnEnable(m_wndParent);
		}
		m_progress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progressVal", (string)null);
		m_lbDay = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_day", (string)null);
		m_grayMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "gray_mask", (string)null);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(zbChapterOneZhanLingCsvRecord record)
	{
		m_record = record;
		InitAwards();
	}

	private void InitAwards()
	{
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbDay).text = m_record.level.ToString();
		JourneyZhanLingDetail zhanLing = zbServerData.sGameData.JourneyInfo.ChapterOne.ZhanLing;
		int journeyOneMidGoodsId = zbSelf.Instance.journeyOneHandler.journeyOneMidGoodsId;
		int journeyOneHighGoodsId = zbSelf.Instance.journeyOneHandler.journeyOneHighGoodsId;
		bool flag = zbSelf.Instance.journeyOneHandler.IsZhanlingActive(journeyOneMidGoodsId);
		bool flag2 = zbSelf.Instance.journeyOneHandler.IsZhanlingActive(journeyOneHighGoodsId);
		bool flag3 = zhanLing.Level < m_record.level;
		bool flag4 = zhanLing.Level == m_record.level;
		bool flag5 = m_record.level <= zhanLing.LowAward;
		bool flag6 = flag && m_record.level <= zbSelf.Instance.journeyOneHandler.GetZhanlingDetailByGoodsId(journeyOneMidGoodsId).HighAward;
		bool flag7 = flag2 && m_record.level <= zbSelf.Instance.journeyOneHandler.GetZhanlingDetailByGoodsId(journeyOneHighGoodsId).HighAward;
		bool item = !flag5 && !flag3;
		bool item2 = (!flag6 && !flag3) & flag;
		bool item3 = (!flag7 && !flag3) & flag2;
		(int[], bool, bool)[] array = new(int[], bool, bool)[3]
		{
			(m_record.lowAward, flag5, item),
			(m_record.mediumAward, flag6, item2),
			(m_record.highAward, flag7, item3)
		};
		for (int i = 0; i < array.Length && i < m_awards.Length; i++)
		{
			zbItem obj = m_awards[i];
			int[] item4 = array[i].Item1;
			bool item5 = array[i].Item2;
			bool item6 = array[i].Item3;
			zbItemData data = new zbItemData((EResourceType)item4[0], item4[1], item4[2]);
			obj.Refresh(data);
			obj.SetGettedVisible(item5);
			obj.SetSpineVisible(item6);
			bool canReceiveCopy = item6;
			int[] dataCopy = item4;
			obj.SetTouchCallback(delegate
			{
				if (canReceiveCopy)
				{
					zbWsMsgHandler.Instance.Journey_ChapterOneZhanLingAward_Req(m_record.level);
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
			case 2:
				visible = flag3 || !flag2;
				break;
			}
			((VisualElement)obj2).visible = visible;
			m_progress.style.height = new StyleLength(Length.Percent((float)((!flag3) ? (flag4 ? 50 : 100) : 0)));
			m_grayMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
		}
	}
}
