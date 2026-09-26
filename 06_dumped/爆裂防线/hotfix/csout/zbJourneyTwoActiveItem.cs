using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJourneyTwoActiveItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbJourneyTwoActiveItem>
	{
	}

	private zbJourneyTwoActiveWnd m_wndParent;

	private int m_index;

	private VisualElement m_bg;

	private Label m_lbActive;

	private zbItem[] m_items = new zbItem[3];

	private Button m_btnTouch;

	private VisualElement m_spriteGetted;

	private VisualElement m_notify;

	private zbChapterTwoActiveCsvRecord m_record;

	private JourneyTargetRecordDetail m_data;

	public zbJourneyTwoActiveItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/journey/journeyTwo/tb_journeyTwoActive.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbJourneyTwoActiveWnd wndParent)
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "root", (string)null);
		m_lbActive = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_active", (string)null);
		for (int i = 0; i < m_items.Length; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, $"item{i}", (string)null);
			m_items[i]?.OnEnable(m_wndParent);
		}
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_spriteGetted = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_getted", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(-5f, -5f), 1f, true);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(zbChapterTwoActiveCsvRecord record)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		((TextElement)m_lbActive).text = zbUtil.Language("lang.11104") + ": " + m_record.active;
		for (int i = 0; i < m_items.Length; i++)
		{
			if (m_items[i] == null)
			{
				continue;
			}
			if (record != null && i < record.awardList.Length)
			{
				int[] array = record.awardList[i];
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

	public void Refresh()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		m_data = ((IEnumerable<JourneyTargetRecordDetail>)zbServerData.sGameData.JourneyInfo.ChapterTwo.Active.TargetList).FirstOrDefault((JourneyTargetRecordDetail v) => v.Target == m_record.active);
		if (m_data == null)
		{
			return;
		}
		m_bg.style.backgroundImage = ((m_data.State == ECommonState.End) ? m_wndParent.m_getAwardBg : m_wndParent.m_notGetAwardBg);
		m_spriteGetted.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.State != ECommonState.End));
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.State != ECommonState.Yes));
		for (int num = 0; num < m_items.Length; num++)
		{
			if (m_items[num] == null)
			{
				continue;
			}
			m_items[num].SetGettedVisible(m_data.State == ECommonState.End);
			m_items[num].SetSpineVisible(m_data.State == ECommonState.Yes);
			if (m_data.State == ECommonState.Yes)
			{
				m_items[num].SetTouchCallback(delegate
				{
					OnTouch();
				});
			}
			else
			{
				m_items[num].SetTouchCallback(null);
			}
		}
	}

	private void OnTouch()
	{
		if (m_data != null)
		{
			if (m_data.State == ECommonState.No)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11139"));
			}
			else if (m_data.State == ECommonState.Yes)
			{
				zbWsMsgHandler.Instance.Journey_ChapterTwoActive_Req(m_data.Target);
			}
		}
	}
}
