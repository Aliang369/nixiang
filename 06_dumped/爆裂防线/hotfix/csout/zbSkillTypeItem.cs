using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSkillTypeItem : VisualElement
{
	private Label m_lbName;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private VisualElement m_skillTypeIcon;

	private VisualElement m_mask;

	private Label m_lbCount;

	private zbEncyclopediaWnd m_wndParent;

	private zbModHandbookTypeCsvRecord m_record;

	private ModHandbookTypeTarget m_modSkillTypeTarget;

	private bool m_isUnlocked;

	public zbSkillTypeItem(zbEncyclopediaWnd wndParent)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)this)._002Ector();
		m_wndParent = wndParent;
		string resName = "ui/uxml/encyclopedia/tb_skillTypeItem.uxml";
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>(resName);
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_skillTypeName", (string)null);
		m_skillTypeIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "skillTypeIcon", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_count", (string)null);
		m_mask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "mask", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(20f, 10f), 1f, true);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbModHandbookTypeCsvRecord record)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_skillTypeIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.icon));
		((TextElement)m_lbName).text = zbUtil.Language(m_record.name);
		RefreshStatus();
		UpdateNotify();
	}

	private void RefreshStatus()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		m_isUnlocked = IsCurrentTypeUnlocked();
		m_mask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_isUnlocked ? 1 : 0));
		m_modSkillTypeTarget = zbSelf.Instance.handbookHandler.GetModHandbookTypeTargetDetail(m_record.type);
		int totalModsOfCurrentType = GetTotalModsOfCurrentType();
		if (m_isUnlocked && m_modSkillTypeTarget != null)
		{
			((TextElement)m_lbCount).text = $"{m_modSkillTypeTarget.Count}/{totalModsOfCurrentType}";
		}
		else
		{
			((TextElement)m_lbCount).text = $"0/{totalModsOfCurrentType}";
		}
	}

	private void UpdateNotify()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		bool num = IsCurrentTypeUnlocked();
		bool flag = false;
		if (num)
		{
			bool flag2 = zbSelf.Instance.handbookHandler.CanGetHandbookTypeTargetAward(m_record.type);
			foreach (zbModHandbookCsvRecord value in zbDataConfig.Instance.modHandbookCsvTable.GetRecords().records.Values)
			{
				if (value.type == m_record.type && zbSelf.Instance.handbookHandler.CanModActive(value.id))
				{
					flag = true;
					break;
				}
			}
			flag |= flag2;
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	private bool IsCurrentTypeUnlocked()
	{
		WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType((ESkillType)m_record.type);
		if (warRoomDetailBySkillType != null)
		{
			return warRoomDetailBySkillType.HeroTid > 0;
		}
		return false;
	}

	private int GetTotalModsOfCurrentType()
	{
		return zbDataConfig.Instance.modHandbookCsvTable.GetRecords().records.Values.Count((zbModHandbookCsvRecord r) => r.type == m_record.type);
	}

	public void OnTouch(ClickEvent evt)
	{
		if (m_record != null && m_isUnlocked)
		{
			zbUIMgr.Instance.ShowModInfoList(m_record);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10835"));
		}
	}
}
