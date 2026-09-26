using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLeaderItemEquip : zbViewBaseItem
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private VisualElement m_subLevel;

	private VisualElement m_spriteLock;

	private VisualElement m_iconNew;

	private Label m_lbSubLevel;

	private VisualElement m_spriteSelect;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private zbBaseWnd m_wndParent;

	private zbLeaderTabEquip m_tabParent;

	private EquipDetail m_equip;

	public zbLeaderItemEquip(zbBaseWnd wndParent, zbLeaderTabEquip tabParent)
	{
		m_wndParent = wndParent;
		m_tabParent = tabParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/leader/tb_leader_equip.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_subLevel = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "subLevel", (string)null);
		m_lbSubLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_subLevel", (string)null);
		m_spriteSelect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "select", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "touch", (string)null);
		m_spriteLock = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "lock", (string)null);
		m_iconNew = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_new", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, null, 1f, scaleAnim: false);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(EquipDetail data)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		m_equip = data;
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex(m_equip.Quality);
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceFrameTex);
		if ((Object)(object)val != (Object)null)
		{
			m_spriteFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_spriteFrame.MarkDirtyRepaint();
		}
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Equip, m_equip.EquipTid);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resorceIconPath.strIconPath);
		if ((Object)(object)val2 != (Object)null)
		{
			m_spriteIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteIcon.MarkDirtyRepaint();
		}
		m_subLevel.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_equip.SubLevel <= 0));
		((TextElement)m_lbSubLevel).text = m_equip.SubLevel.ToString();
		m_spriteLock.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_equip.Lock != EBoolState.True));
		bool flag = zbSelf.Instance.equipHandler.CanEquipSet(m_equip.EquipId);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		bool flag2 = zbSelf.Instance.newTagHandler.IsNewEquip(data.EquipId);
		m_iconNew.visible = flag2 && !flag;
	}

	public void RefreshSelect(string selectedId)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		m_spriteSelect.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(m_equip.EquipId == selectedId)));
	}

	public void OnTouch(ClickEvent evt)
	{
		zbSelf.Instance.newTagHandler.SetNewEquipViewed(m_equip.EquipId);
		m_iconNew.visible = false;
		m_tabParent.RefreshSelect(m_equip.EquipId);
		zbUIMgr.Instance.ShowEquip(m_equip);
	}
}
