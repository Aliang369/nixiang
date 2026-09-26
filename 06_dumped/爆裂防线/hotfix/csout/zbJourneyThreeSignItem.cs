using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJourneyThreeSignItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbJourneyThreeSignItem>
	{
	}

	private zbJourneyThreeSignWnd m_wndParent;

	private VisualElement m_bg;

	private Label m_lbDay;

	private zbItem[] m_items = new zbItem[3];

	private VisualElement m_spriteProgress;

	private Label m_lbProgress;

	private Button m_btnTouch;

	private VisualElement m_spriteGetted;

	private VisualElement m_notify;

	private zbChapterThreeSignCsvRecord m_record;

	private JourneyTargetRecordDetail m_data;

	public zbJourneyThreeSignItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/journey/journeyThree/tb_journeyThreeSign.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbJourneyThreeSignWnd wndParent)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_lbDay = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_day", (string)null);
		for (int i = 0; i < m_items.Length; i++)
		{
			m_items[i] = UQueryExtensions.Q<zbItem>((VisualElement)(object)this, $"item{i}", (string)null);
			m_items[i]?.OnEnable(m_wndParent);
		}
		m_spriteProgress = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_progress", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_spriteGetted = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_getted", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(-5f, -5f), 1f, true);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(zbChapterThreeSignCsvRecord record)
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		((TextElement)m_lbDay).text = "0" + m_record.day;
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
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		m_data = ((IEnumerable<JourneyTargetRecordDetail>)zbServerData.sGameData.JourneyInfo.ChapterThree.Sign.TargetList).FirstOrDefault((JourneyTargetRecordDetail v) => v.Target == m_record.openCount);
		if (m_data == null)
		{
			return;
		}
		m_bg.style.backgroundImage = ((m_data.State == ECommonState.End) ? m_wndParent.m_getAwardBg : m_wndParent.m_notGetAwardBg);
		m_spriteGetted.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_data.State != ECommonState.End && m_record.day <= zbServerData.sGameData.JourneyInfo.ChapterThree.Sign.Day) ? 1 : 0));
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
		float num2 = Mathf.Min((float)zbServerData.sGameData.JourneyInfo.ChapterThree.Sign.Reach / (float)m_data.Target, 1f);
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(num2 * 304f, (LengthUnit)0));
		((TextElement)m_lbProgress).text = $"{zbServerData.sGameData.JourneyInfo.ChapterThree.Sign.Reach}/{m_data.Target}";
	}

	private void OnTouch()
	{
		if (m_record.day > zbServerData.sGameData.JourneyInfo.ChapterThree.Sign.Day)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11177"), m_record.day - zbServerData.sGameData.JourneyInfo.ChapterThree.Sign.Day));
		}
		else if (m_data != null)
		{
			if (m_data.State == ECommonState.No)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11178"));
			}
			else if (m_data.State == ECommonState.Yes)
			{
				zbWsMsgHandler.Instance.Journey_ChapterThreeSign_Req(m_data.Index);
			}
		}
	}
}
