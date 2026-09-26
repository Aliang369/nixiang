using System;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerInfoItem
{
	private zbPlayerInfoWnd m_wndParent;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteEquip;

	private Label m_lbSubLevel;

	private Label m_lbLevel;

	private Button m_btnTouch;

	private VisualElement[] m_spriteGems = (VisualElement[])(object)new VisualElement[10];

	private EquipSlotInfo slotInfo;

	private EquipSlotDetail m_data;

	private EEquipType equipType;

	public RepeatedField<GemSlotDetail> GemSlotList;

	public void OnEnable(zbPlayerInfoWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_spriteFrame = root;
		m_spriteEquip = UQueryExtensions.Q<VisualElement>(m_spriteFrame, "icon_equip", (string)null);
		m_lbSubLevel = UQueryExtensions.Q<Label>(m_spriteFrame, "lb_subLevel", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_spriteFrame, "lb_level", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_spriteFrame, "btn_touch", (string)null);
		for (int i = 0; i < 10; i++)
		{
			m_spriteGems[i] = UQueryExtensions.Q<VisualElement>(m_spriteFrame, "gem" + i, (string)null);
		}
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
	}

	public void Refresh(EquipSlotInfo _slotInfo, EEquipType _equipType)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		slotInfo = _slotInfo;
		equipType = _equipType;
		m_data = zbSelf.Instance.equipHandler.GetEquipSlotInfo(equipType, _slotInfo);
		if (m_data == null)
		{
			((TextElement)m_lbLevel).text = "";
			m_spriteEquip.visible = false;
			return;
		}
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
		GemSlotList = zbSelf.Instance.equipHandler.GetGemSlotList(m_data.EquipType, slotInfo);
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
	}

	public void OnTouch(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowEquipSlot(slotInfo, equipType, m_wndParent.IsSelf());
	}
}
