using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbModInfoItem : zbViewBaseItem
{
	private zbModInfoWnd m_wndParent;

	private Button m_btnTouch;

	private VisualElement m_modIcon;

	private Label m_lbModName;

	private Label m_lbModDesc;

	private VisualElement m_bubble;

	private VisualElement m_bg;

	private int m_modHandbookId;

	private VisualElement m_notify;

	private zbModHandbookCsvRecord m_modDetail;

	public zbModInfoItem(zbModInfoWnd parent, VisualTreeAsset tAsset)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_wndParent = parent;
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_modIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "modIcon", (string)null);
		m_lbModName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_modName", (string)null);
		m_lbModDesc = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_modDesc", (string)null);
		m_bubble = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bubble", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_notify = zbUtil.CreateNotifyTag(m_bg, (Vector2?)new Vector2(-13f, -10f), 1f, true);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(zbModHandbookCsvRecord mod)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		m_modDetail = mod;
		m_modHandbookId = mod.id;
		m_modIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(mod.icon));
		((TextElement)m_lbModName).text = mod.name;
		((TextElement)m_lbModDesc).text = mod.desc;
		bool flag = zbSelf.Instance.handbookHandler.CanModActive(m_modHandbookId);
		bool flag2 = zbSelf.Instance.handbookHandler.CanModDone(m_modHandbookId);
		((VisualElement)m_btnTouch).style.opacity = StyleFloat.op_Implicit((mod.unlock == 1 || (mod.unlock == 0 && (flag | flag2))) ? 0f : 1f);
		m_bubble.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((mod.unlock != 0 || flag2) ? 1 : 0));
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	private void OnTouch(ClickEvent e)
	{
		if (zbSelf.Instance.handbookHandler.CanModActive(m_modHandbookId))
		{
			zbWsMsgHandler.Instance.Handbook_ModActiveAward_Req(m_modHandbookId);
		}
		else
		{
			zbUIMgr.Instance.ShowModDetail(m_modDetail);
		}
	}
}
