using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFootholdItem : zbViewBaseItem
{
	private zbFootholdWnd m_wndParent;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private VisualElement m_spriteMask;

	private VisualElement m_nodeStar;

	private VisualElement[] m_stars;

	private VisualElement m_spriteSelect;

	private VisualElement m_spriteUsed;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private zbFootholdCsvRecord m_record;

	private FootholdDetail m_data;

	public zbFootholdItem(zbFootholdWnd wndParent)
	{
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		m_stars = (VisualElement[])(object)new VisualElement[5];
		base._002Ector();
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/guard/tb_foothold.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_spriteMask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_mask", (string)null);
		m_nodeStar = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "node_star", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_stars[i] = UQueryExtensions.Q<VisualElement>(m_nodeStar, "star" + i, (string)null);
		}
		m_spriteSelect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_select", (string)null);
		m_spriteUsed = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_used", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, (Vector2?)new Vector2(-3f, -2f), 1f, true);
	}

	public void InitRecord(zbFootholdCsvRecord record)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/wall/wall_frame{m_record.quality}.png"));
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(record.icon));
	}

	public void Refresh()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		m_data = zbSelf.Instance.guardHandler.GetFootholdDetail(m_record.id);
		m_nodeStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_data == null || m_record.open == 1) ? 1 : 0));
		m_spriteMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data != null));
		m_spriteUsed.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_record.id != zbServerData.sGameData.RoleBaseInfo.FootholdId) ? 1 : 0));
		if (m_data != null)
		{
			zbUIEffect.SetStars(m_stars, m_data.Star);
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.guardHandler.HasFootholdNotify(m_record.id)));
		m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_record.id != m_wndParent.m_selectId) ? 1 : 0));
	}

	public void OnTouch()
	{
		m_wndParent.RefreshSelectList(m_record.id);
	}
}
