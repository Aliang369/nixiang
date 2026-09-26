using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJourneyOneXunLuoItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbJourneyOneXunLuoItem>
	{
	}

	private zbJourneyOneXunLuoWnd m_wndParent;

	private int m_index;

	private VisualElement m_node;

	private Button m_btn0;

	private Button m_btn1;

	private VisualElement m_mask;

	private VisualElement m_spriteBubble;

	private VisualElement m_notify;

	private Label m_lbDay;

	private zbItem[] m_items = new zbItem[3];

	private StyleBackground m_notGetAwardBg;

	private StyleBackground m_getAwardBg;

	public zbJourneyOneXunLuoItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/journey/journeyOne/tb_journeyOneXunLuo.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbJourneyOneXunLuoWnd wndParent)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_node = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "day", (string)null);
		m_btn0 = UQueryExtensions.Q<Button>(m_node, "btn_0", (string)null);
		m_btn1 = UQueryExtensions.Q<Button>(m_node, "btn_1", (string)null);
		m_mask = UQueryExtensions.Q<VisualElement>(m_node, "spriteGet", (string)null);
		m_spriteBubble = UQueryExtensions.Q<VisualElement>(m_node, "spriteTip", (string)null);
		m_lbDay = UQueryExtensions.Q<Label>(m_node, "lb_day", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btn0, (Vector2?)new Vector2(43f, 10f), 1f, true);
		for (int i = 0; i < m_items.Length; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>(m_node, $"item{i}", (string)null);
			m_items[i]?.OnEnable(m_wndParent);
		}
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btn0, (Action<ClickEvent>)delegate
		{
			OnClick();
		}, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btn1, (Action<ClickEvent>)delegate
		{
			OnClick();
		}, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(int index)
	{
		m_index = index;
		((TextElement)m_lbDay).text = "0" + (index + 1);
	}

	public void SetBackgrounds(StyleBackground notGetAwardBg, StyleBackground getAwardBg)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		m_notGetAwardBg = notGetAwardBg;
		m_getAwardBg = getAwardBg;
	}

	public void RefreshAwards(int[][] awards)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_items.Length; i++)
		{
			if (m_items[i] == null)
			{
				continue;
			}
			if (awards != null && i < awards.Length)
			{
				int[] array = awards[i];
				if (array != null && array.Length >= 3)
				{
					zbItemData data = new zbItemData((EResourceType)array[0], array[1], array[2]);
					m_items[i].Refresh(data);
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

	public void SetVisible(bool visible)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!visible));
	}

	public void RefreshState(bool isToday, bool isAvailableToday, bool isReceived, bool canMakeup)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		m_node.style.backgroundImage = (isReceived ? m_getAwardBg : m_notGetAwardBg);
		m_mask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!isReceived));
		m_spriteBubble.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!canMakeup));
		((VisualElement)m_btn1).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!isAvailableToday));
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!isAvailableToday));
		for (int i = 0; i < m_items.Length; i++)
		{
			if (m_items[i] != null)
			{
				m_items[i].SetGettedVisible(isReceived);
				m_items[i].SetSpineVisible(isAvailableToday);
			}
		}
	}

	private void RefreshStateComputed(int day, int currentDay)
	{
		bool flag = day == currentDay;
		bool isReceived = IsDayReceived(day);
		bool num = IsDayAvailable(day);
		bool isAvailableToday = num & flag;
		bool canMakeup = num && day < currentDay;
		RefreshState(flag, isAvailableToday, isReceived, canMakeup);
	}

	public void RefreshAll(int day, int currentDay, int[][] awards)
	{
		RefreshAwards(awards);
		RefreshStateComputed(day, currentDay);
	}

	public void UpdateStateTick(int day, int currentDay)
	{
		RefreshStateComputed(day, currentDay);
	}

	private void OnClick()
	{
		OnGetAward(m_index);
	}

	private void OnGetAward(int index)
	{
		int day = index + 1;
		if (IsDayAvailable(day) && day == zbServerData.sGameData?.JourneyInfo?.ChapterOne?.Sign?.Reach)
		{
			zbWsMsgHandler.Instance.Journey_ChapterOneSign_Req(day);
		}
		else if (IsDayAvailable(day) && day < zbServerData.sGameData?.JourneyInfo?.ChapterOne?.Sign?.Reach)
		{
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10143"), zbUtil.Language("lang.10812"), delegate(bool isOK)
			{
				if (isOK)
				{
					zbWsMsgHandler.Instance.Journey_ChapterOneSign_Req(day);
				}
			});
		}
		else if (IsDayReceived(day) && day <= zbServerData.sGameData?.JourneyInfo?.ChapterOne?.Sign?.Reach)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10157"));
		}
		else if (!IsDayAvailable(day) && !IsDayReceived(day))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10813"));
		}
	}

	private bool IsDayAvailable(int day)
	{
		JourneyStatisticsDetail journeyStatisticsDetail = zbServerData.sGameData?.JourneyInfo?.ChapterOne?.Sign;
		if (journeyStatisticsDetail == null || journeyStatisticsDetail.TargetList == null)
		{
			return false;
		}
		foreach (JourneyTargetRecordDetail target in journeyStatisticsDetail.TargetList)
		{
			if (target.Index == day)
			{
				return target.State == ECommonState.Yes;
			}
		}
		return false;
	}

	private bool IsDayReceived(int day)
	{
		JourneyStatisticsDetail journeyStatisticsDetail = zbServerData.sGameData?.JourneyInfo?.ChapterOne?.Sign;
		if (journeyStatisticsDetail == null || journeyStatisticsDetail.TargetList == null)
		{
			return false;
		}
		foreach (JourneyTargetRecordDetail target in journeyStatisticsDetail.TargetList)
		{
			if (target.Index == day)
			{
				return target.State == ECommonState.End;
			}
		}
		return false;
	}
}
