using System;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEquipSlotItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbEquipSlotItem>
	{
	}

	private zbEquipUpgradeWnd m_wndParent;

	private EEquipType m_equipType;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteEquip;

	private Label m_lbSubLevel;

	private Label m_lbLevel;

	private Button m_btnTouch;

	private VisualElement[] m_spriteGems = (VisualElement[])(object)new VisualElement[10];

	private VisualElement m_spriteSelect;

	private VisualElement m_notify;

	private EquipSlotDetail m_data;

	public RepeatedField<GemSlotDetail> GemSlotList = new RepeatedField<GemSlotDetail>();

	public zbEquipSlotItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/ctrl/tb_equipSlot.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbEquipUpgradeWnd wndParent, EEquipType equipType)
	{
		m_wndParent = wndParent;
		m_equipType = equipType;
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_spriteEquip = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_equip", (string)null);
		m_lbSubLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_subLevel", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		for (int i = 0; i < 10; i++)
		{
			m_spriteGems[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "gem" + i, (string)null);
		}
		m_spriteSelect = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "select", (string)null);
		m_spriteSelect.visible = false;
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		m_data = zbSelf.Instance.equipHandler.GetEquipSlotInfo(m_equipType);
		((TextElement)m_lbLevel).text = "Lv." + m_data.Level;
		((VisualElement)m_lbSubLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.Equip == null));
		string resName = "ui/image/frame/bg_frame_1.png";
		if (m_data.Equip != null)
		{
			((TextElement)m_lbSubLevel).text = m_data.Equip.SubLevel.ToString();
			resName = "ui/image/frame/bg_frame_" + (int)m_data.Equip.Quality + ".png";
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(EResourceType.Equip, m_data.Equip.EquipTid);
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resorceIconPath.strIconPath);
			if ((Object)(object)val != (Object)null)
			{
				m_spriteEquip.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
				m_spriteEquip.MarkDirtyRepaint();
			}
			m_spriteEquip.visible = true;
		}
		else
		{
			m_spriteEquip.visible = false;
		}
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resName);
		if ((Object)(object)val2 != (Object)null)
		{
			m_spriteFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteFrame.MarkDirtyRepaint();
		}
		GemSlotList = zbSelf.Instance.equipHandler.GetGemSlotList(m_equipType);
		foreach (GemSlotDetail gemSlot in GemSlotList)
		{
			string resName2 = "ui/image/icon/gemstone/gem_type" + (int)gemSlot.GemType + ".png";
			if (gemSlot.GemTid > 0)
			{
				resName2 = zbResourceHelper.GetResorceIconPath(EResourceType.Gem, gemSlot.GemTid).strIconPath;
			}
			Texture2D val3 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resName2);
			m_spriteGems[gemSlot.Index].style.backgroundImage = StyleBackground.op_Implicit(val3);
		}
		RefreshNotify();
	}

	public void RefreshNotify()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (m_wndParent.m_currTab == 0)
		{
			m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.equipHandler.CanEquipSlotUpgrade(m_equipType)));
		}
		else
		{
			m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.equipHandler.CanEquipLevelUpgrade(m_equipType)));
		}
	}

	public void RefreshSelect(EEquipType type)
	{
		bool visible = m_equipType == type;
		m_spriteSelect.visible = visible;
	}

	public void OnTouch(ClickEvent evt)
	{
		m_wndParent.RefreshSelectList(m_equipType);
	}
}
