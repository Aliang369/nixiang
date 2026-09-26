using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeadItem : VisualElement
{
	public zbPlayerHeadCsvRecord m_headData;

	public zbPlayerHeadFrameCsvRecord m_headFrameData;

	private zbHeadWnd m_wndParent;

	private zbTabHead m_tabHead;

	private zbTabHeadFrame m_tabHeadFrame;

	private VisualElement m_spriteInuse;

	private VisualElement m_headframe;

	private VisualElement m_head;

	private VisualElement m_Mask;

	private VisualElement m_Select_Box;

	private Button m_btnSelect;

	private VisualElement m_notifyHead;

	private VisualElement m_newTag;

	private bool m_bCallbackRegistered;

	public zbHeadItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_HeadAndFrame.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_Mask = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "Mask", (string)null);
		m_notifyHead = zbUtil.CreateNotifyTag(m_Mask);
	}

	public void OnEnable(zbHeadWnd wndParent, object tab)
	{
		m_wndParent = wndParent;
		if (tab is zbTabHead tabHead)
		{
			m_tabHead = tabHead;
		}
		else if (tab is zbTabHeadFrame tabHeadFrame)
		{
			m_tabHeadFrame = tabHeadFrame;
		}
		if (!m_bCallbackRegistered)
		{
			m_headframe = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "VisualElement_HeadFrame", (string)null);
			m_head = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "VisualElement_Head", (string)null);
			m_Select_Box = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "Select_Box", (string)null);
			m_btnSelect = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_select", (string)null);
			m_spriteInuse = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "spriteInUse", (string)null);
			m_newTag = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "newTag", (string)null);
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSelect, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
			m_bCallbackRegistered = true;
		}
	}

	public void RefreshHead(zbPlayerHeadCsvRecord headRecord)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		m_headData = headRecord;
		m_headframe.visible = false;
		m_head.visible = true;
		m_notifyHead.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_Mask.visible = false;
		bool visible = m_headData.id == zbServerData.sGameData.RoleBaseInfo.HeadId;
		m_Select_Box.visible = visible;
		m_spriteInuse.visible = visible;
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(m_headData.icon);
		m_head.style.backgroundImage = new StyleBackground(val);
		bool flag = zbSelf.Instance.headHandler.IsHeadUnlocker(m_headData.id);
		m_Mask.visible = !flag;
		bool flag2 = m_headData.id != 1 && zbSelf.Instance.newTagHandler.IsNewAvatar(m_headData.id);
		m_newTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
	}

	public void RefreshHeadFrame(zbPlayerHeadFrameCsvRecord headFrameRecord)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		m_headFrameData = headFrameRecord;
		m_notifyHead.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_Mask.visible = false;
		m_Select_Box.visible = false;
		m_spriteInuse.visible = false;
		m_head.visible = false;
		m_headframe.visible = true;
		m_newTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(m_headFrameData.icon);
		m_headframe.style.backgroundImage = new StyleBackground(val);
		bool visible = m_headFrameData.id == zbServerData.sGameData.RoleBaseInfo.HeadFrameId;
		m_Select_Box.visible = visible;
		m_spriteInuse.visible = visible;
		bool flag = zbSelf.Instance.headHandler.IsHeadFrameUnlocker(m_headFrameData.id);
		m_Mask.visible = !flag;
		bool flag2 = zbSelf.Instance.headHandler.HasHeadFrame(m_headFrameData.propId);
		m_notifyHead.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
	}

	private void OnTouch(ClickEvent evt)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (m_tabHead != null)
		{
			zbSelf.Instance.headHandler.OnAvatarViewed(m_headData.id);
			m_newTag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_tabHead.OnHeadSel(m_headData.id);
			m_wndParent.RefreshNotify(null);
		}
		else if (m_tabHeadFrame != null)
		{
			m_tabHeadFrame.OnHeadFrameSel(m_headFrameData.id);
		}
	}

	public void SetSelected(bool isSelected)
	{
		m_Select_Box.visible = isSelected;
	}
}
