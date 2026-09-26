using System;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLeaderEquipSlotItem : zbViewBaseItem
{
	public class UxmlFactory : UxmlFactory<zbLeaderEquipSlotItem>
	{
	}

	private zbLeaderWnd m_wndParent;

	private EEquipType m_equipType;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteEquip;

	private Label m_lbSubLevel;

	private Label m_lbLevel;

	private Button m_btnTouch;

	private VisualElement[] m_spriteGems = (VisualElement[])(object)new VisualElement[10];

	private VisualElement m_spriteSelect;

	private EquipSlotDetail m_data;

	public RepeatedField<GemSlotDetail> GemSlotList;

	public zbLeaderEquipSlotItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/ctrl/tb_equipSlot.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbLeaderWnd wndParent, EEquipType equipType)
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
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
		zbUtil.FixAllBackgrounds((VisualElement)(object)this);
	}

	public void Refresh()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		m_data = zbSelf.Instance.equipHandler.GetEquipSlotInfo(m_equipType);
		((TextElement)m_lbLevel).text = "Lv." + m_data.Level;
		((VisualElement)m_lbSubLevel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_data.Equip == null));
		string resName = "ui/image/frame/bg_frame_1.png";
		if (m_data.Equip != null)
		{
			zbSelf.Instance.newTagHandler.AddNewEquip(m_data.Equip.EquipId);
			zbSelf.Instance.newTagHandler.SetNewEquipViewed(m_data.Equip.EquipId);
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
			((TextElement)m_lbLevel).text = "";
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
			if ((Object)(object)val3 != (Object)null)
			{
				m_spriteGems[gemSlot.Index].style.backgroundImage = new StyleBackground(Background.FromTexture2D(val3));
				m_spriteGems[gemSlot.Index].MarkDirtyRepaint();
			}
		}
	}

	public void OnTouch(ClickEvent evt)
	{
		m_data = zbSelf.Instance.equipHandler.GetEquipSlotInfo(m_equipType);
		if (m_data.Equip != null)
		{
			zbUIMgr.Instance.ShowEquipSlot(zbServerData.sGameData.EquipSlotInfo, m_equipType);
		}
	}
}
