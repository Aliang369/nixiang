using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHomeEditBagItem : zbViewBaseItem
{
	private Label m_lbLevel;

	private Label m_lbName;

	private VisualElement m_spriteIcon;

	private VisualElement[] m_spriteBoxs = (VisualElement[])(object)new VisualElement[3];

	private Button m_btnTouch;

	private zbHomeEditBagWnd m_wndParent;

	private HomelandRoomDetail m_data;

	private zbHomeRoomTypeCsvRecord m_record;

	public zbHomeEditBagItem(zbHomeEditBagWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_homeEditBag.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "bg_icon", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_spriteBoxs[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "box" + i, (string)null);
		}
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(HomelandRoomDetail data)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		((TextElement)m_lbLevel).text = "Lv." + data.Level;
		m_record = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(data.TypeId);
		if (m_record != null)
		{
			((TextElement)m_lbName).text = m_record.name;
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(m_record.iconBuild);
			if ((Object)(object)val != (Object)null)
			{
				m_spriteIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
				m_spriteIcon.MarkDirtyRepaint();
			}
			for (int i = 0; i < 3; i++)
			{
				m_spriteBoxs[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((i >= m_record.length) ? 1 : 0));
			}
			zbUtil.CenterNodeItems(m_spriteBoxs, 33f);
		}
	}

	public void OnTouch(ClickEvent evt)
	{
		if ((Object)(object)m_wndParent != (Object)null && m_data != null)
		{
			zbHomeEditWnd zbHomeEditWnd2 = zbUIMgr.Instance.GetWndById(EWndId.HomeEdit) as zbHomeEditWnd;
			zbHomeEditRoom canSetRoom = zbHomeEditWnd2.GetCanSetRoom(m_record.length);
			if (canSetRoom != null)
			{
				m_wndParent.Hide();
				m_data.X = canSetRoom.x;
				m_data.Y = canSetRoom.y;
				zbSelf.Instance.homeEditHandler.RemoveFromBag(m_data);
				zbSelf.Instance.homeEditHandler.curEditRoomId = m_data.RoomId;
				zbHomeEditWnd2.RefreshDetail();
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10104"));
			}
		}
	}
}
