using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantSubSkill : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbServantSubSkill>
	{
	}

	private zbBaseWnd m_wndParent;

	private VisualElement m_frame;

	private VisualElement m_icon;

	private VisualElement m_iconNew;

	private Label m_lbLv;

	private Button m_btnTouch;

	private ServantSkillDetail m_detail;

	public zbServantSubSkill()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/servant/tb_subSkill.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbBaseWnd wndParent)
	{
		m_wndParent = wndParent;
		m_frame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_icon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_iconNew = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_new", (string)null);
		m_lbLv = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_lv", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(ServantSkillDetail detail, bool isNewAdded = false)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		m_detail = detail;
		bool flag = m_detail == null;
		m_icon.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		((VisualElement)m_lbLv).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		m_iconNew.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!isNewAdded));
		if (!flag)
		{
			zbServantSkillCsvRecord recordByKey = zbDataConfig.Instance.servantSkillCsvTable.GetRecordByKey(m_detail.SkillId);
			m_icon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
			((TextElement)m_lbLv).text = detail.Level.ToString();
		}
	}

	public void OnTouch(ClickEvent evt)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if (m_detail != null)
		{
			zbServantSkillCsvRecord recordByKey = zbDataConfig.Instance.servantSkillCsvTable.GetRecordByKey(m_detail.SkillId);
			IEventHandler target = ((EventBase)evt).target;
			Rect worldBound = ((VisualElement)((target is VisualElement) ? target : null)).worldBound;
			Vector2 position = ((Rect)(ref worldBound)).position;
			Vector2 pos = VisualElementExtensions.WorldToLocal(UQueryExtensions.Q<VisualElement>(m_wndParent.GetRootElement(), "root", (string)null), position);
			pos.x += 60f;
			pos.y += 30f;
			zbUIMgr.Instance.ShowServantSubSkillTip(zbUtil.Language(recordByKey.desc), pos);
		}
	}
}
