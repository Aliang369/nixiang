using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbPlayerTipsWallItem : zbViewBaseItem
{
	private zbPlayerTipsDetailWnd m_wndParent;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteWall;

	private VisualElement m_nodeStar;

	private VisualElement[] m_stars = (VisualElement[])(object)new VisualElement[5];

	private Button m_btnTouch;

	private zbWallCsvRecord m_record;

	private WallDetail m_data;

	public zbPlayerTipsWallItem(zbPlayerTipsDetailWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/comm/tb_tipsWall.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_spriteWall = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_nodeStar = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "node_star", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_stars[i] = UQueryExtensions.Q<VisualElement>(m_nodeStar, "star" + i, (string)null);
		}
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
	}

	public void Refresh(WallDetail wallDetail)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		m_data = wallDetail;
		m_record = zbDataConfig.Instance.wallCsvTable.GetRecordByKey(wallDetail.WallId);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/wall/wall_frame{m_record.quality}.png"));
		m_spriteWall.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.icon));
		m_nodeStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((wallDetail == null || m_record.id == 1) ? 1 : 0));
		if (wallDetail != null)
		{
			zbUIEffect.SetStars(m_stars, wallDetail.Star);
		}
	}

	public void OnTouch()
	{
		if (m_record.propId == 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11338"));
		}
		else
		{
			zbUIMgr.Instance.ShowPreview(EPreviewType.Wall, m_record.propId, m_data.Star);
		}
	}
}
