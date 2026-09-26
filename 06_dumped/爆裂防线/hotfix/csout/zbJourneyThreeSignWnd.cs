using System;
using System.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJourneyThreeSignWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbTime;

	private zbJourneyThreeSignItem[] m_items = new zbJourneyThreeSignItem[5];

	private Label m_lbRoom;

	private Button m_btnGoto;

	private Button m_btnClose;

	public StyleBackground m_notGetAwardBg;

	public StyleBackground m_getAwardBg;

	private zbChapterThreeSignCsvRecord[] m_records;

	protected override void OnEnable()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_notGetAwardBg = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/journey/journey1/xunluo/journeyXunLuo_0.png"));
		m_getAwardBg = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/journey/journey1/xunluo/journeyXunLuo_1.png"));
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.11175");
		m_lbRoom = UQueryExtensions.Q<Label>(m_RootElement, "lb_room", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbJourneyThreeSignItem>(m_RootElement, $"sign{i}", (string)null);
			m_items[i].OnEnable(this);
		}
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_JourneyInfo, delegate
		{
			Refresh();
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, delegate
		{
			Refresh();
		});
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Tili
		});
		InitRecords();
	}

	private void InitRecords()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		m_records = zbDataConfig.Instance.chapterThreeSignCsvTable.GetRecords().records.Values.ToArray();
		for (int i = 0; i < m_items.Length; i++)
		{
			if (m_items[i] == null)
			{
				continue;
			}
			if (m_records != null && i < m_records.Length)
			{
				zbChapterThreeSignCsvRecord zbChapterThreeSignCsvRecord2 = m_records[i];
				if (zbChapterThreeSignCsvRecord2 != null)
				{
					m_items[i].Init(zbChapterThreeSignCsvRecord2);
					((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				}
				else
				{
					((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				}
			}
			else
			{
				((VisualElement)m_items[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
	}

	public override void Refresh()
	{
		for (int i = 0; i < m_items.Length; i++)
		{
			m_items[i].Refresh();
		}
		RefreshLeftTime();
		((TextElement)m_lbRoom).text = zbServerData.sGameData.JourneyInfo?.ChapterThree?.Sign.Reach.ToString();
	}

	private void RefreshLeftTime()
	{
		long endLeftTime = zbSelf.Instance.journeyThreeHandler.GetEndLeftTime();
		if (endLeftTime > 0)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(endLeftTime);
		}
		else
		{
			OnClose();
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshLeftTime();
	}

	private void OnClose()
	{
		Hide();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		zbWsMsgHandler.Instance.Journey_Data_Req();
	}
}
