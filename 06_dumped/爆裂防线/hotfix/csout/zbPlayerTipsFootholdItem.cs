using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbPlayerTipsFootholdItem : zbViewBaseItem
{
	private zbPlayerTipsDetailWnd m_wndParent;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private VisualElement m_nodeStar;

	private VisualElement[] m_stars = (VisualElement[])(object)new VisualElement[5];

	private Button m_btnTouch;

	private zbFootholdCsvRecord m_record;

	private FootholdDetail m_data;

	public zbPlayerTipsFootholdItem(zbPlayerTipsDetailWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/comm/tb_tipsFoothold.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_nodeStar = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "node_star", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_stars[i] = UQueryExtensions.Q<VisualElement>(m_nodeStar, "star" + i, (string)null);
		}
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnTouch, "", (TrickleDown)0);
	}

	public void Refresh(FootholdDetail data)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		m_record = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey(data.FootholdId);
		m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/wall/wall_frame{m_record.quality}.png"));
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.icon));
		m_nodeStar.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_record.propId == 0));
		if (m_data != null)
		{
			zbUIEffect.SetStars(m_stars, m_data.Star);
		}
	}

	public void OnTouch()
	{
		if (m_record.propId == 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11338"));
		}
		else if (m_data != null)
		{
			zbUIMgr.Instance.ShowPreview(EPreviewType.Foothold, m_record.propId, m_data.Star);
		}
		else
		{
			zbUIMgr.Instance.ShowPreview(EPreviewType.Foothold, m_record.propId);
		}
	}
}
