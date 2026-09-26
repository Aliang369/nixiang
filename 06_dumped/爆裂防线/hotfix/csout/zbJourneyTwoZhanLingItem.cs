using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbJourneyTwoZhanLingItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbJourneyTwoZhanLingItem>
	{
	}

	private zbBaseWnd m_wndParent;

	private zbItem[] m_awards = new zbItem[3];

	private VisualElement m_progress;

	private Label m_lbDay;

	private VisualElement m_grayMask;

	private zbChapterTwoZhanLingCsvRecord m_record;

	public zbJourneyTwoZhanLingItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/journey/journeyTwo/tb_journeyTwoZhanLing.uxml");
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

	public void Init(zbChapterTwoZhanLingCsvRecord record)
	{
		m_record = record;
		InitAwards();
	}

	private void InitAwards()
	{
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbDay).text = m_record.level.ToString();
		JourneyZhanLingDetail zhanLing = zbServerData.sGameData.JourneyInfo.ChapterTwo.ZhanLing;
		JObject record = zbDataConfig.Instance.chapterTwoBaseJsonTable.GetRecord();
		int goodsId = Extensions.Value<int>((IEnumerable<JToken>)record["zhanLing"][(object)"mediumGoodsId"]);
		int goodsId2 = Extensions.Value<int>((IEnumerable<JToken>)record["zhanLing"][(object)"highGoodsId"]);
		bool flag = zbSelf.Instance.journeyTwoHandler.IsZhanlingActive(goodsId);
		bool flag2 = zbSelf.Instance.journeyTwoHandler.IsZhanlingActive(goodsId2);
		bool flag3 = zhanLing.Level < m_record.level;
		bool flag4 = zhanLing.Level == m_record.level;
		bool flag5 = m_record.level <= zhanLing.LowAward;
		bool flag6 = flag && m_record.level <= zbSelf.Instance.journeyTwoHandler.GetZhanlingDetailByGoodsId(goodsId).HighAward;
		bool flag7 = flag2 && m_record.level <= zbSelf.Instance.journeyTwoHandler.GetZhanlingDetailByGoodsId(goodsId2).HighAward;
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
					zbWsMsgHandler.Instance.Journey_ChapterTwoZhanLingAward_Req(m_record.level);
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
