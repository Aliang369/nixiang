using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWallItem : zbViewBaseItem
{
	private zbWallWnd m_wndParent;

	private zbWallTabSet m_parent;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteWall;

	private VisualElement m_spriteMask;

	private VisualElement m_nodeStar;

	private VisualElement[] m_stars;

	private VisualElement m_spriteSelect;

	private VisualElement m_spriteUsed;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private zbWallCsvRecord m_record;

	private WallDetail m_data;

	public zbWallItem(zbWallWnd wndParent, zbWallTabSet parent)
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		m_stars = (VisualElement[])(object)new VisualElement[5];
		base._002Ector();
		m_wndParent = wndParent;
		m_parent = parent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/wall/tb_wall.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_spriteWall = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
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

	public void Refresh(zbWallCsvRecord record)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		m_record = record;
		m_data = zbSelf.Instance.wallHandler.GetWallDetail(m_record.id);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/wall/wall_frame{m_record.quality}.png"));
		m_spriteWall.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.icon));
		m_nodeStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_data == null || m_record.id == 1) ? 1 : 0));
		m_spriteMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data != null));
		m_spriteUsed.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_record.id != zbServerData.sGameData.WallInfo.WallId) ? 1 : 0));
		if (m_data != null)
		{
			zbUIEffect.SetStars(m_stars, m_data.Star);
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallHandler.HasWallNotify(m_record.id)));
		RefreshSelect();
	}

	public void RefreshSelect()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_record.id != m_parent.m_selectId) ? 1 : 0));
	}

	public void OnTouch()
	{
		m_parent.RefreshSelectList(m_record.id);
	}
}
