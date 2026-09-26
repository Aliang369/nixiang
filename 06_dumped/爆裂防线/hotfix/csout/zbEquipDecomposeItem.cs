using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbEquipDecomposeItem : zbViewBaseItem
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private Label m_lbSubLevel;

	private VisualElement m_spriteOk;

	private Button m_btnTouch;

	private zbEquipDecomposeWnd m_wndParent;

	private EquipDetail m_equip;

	public zbEquipDecomposeItem(zbEquipDecomposeWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/leader/tb_equipDecompose.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_lbSubLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_subLevel", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "sprite_ok", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(EquipDetail data)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
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
		if (m_equip.SubLevel > 0)
		{
			((TextElement)m_lbSubLevel).text = m_equip.SubLevel.ToString();
		}
		else
		{
			((TextElement)m_lbSubLevel).text = "";
		}
		RefreshSelect();
	}

	public void RefreshSelect()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		foreach (EquipDetail select in m_wndParent.selectList)
		{
			if (select.EquipId == m_equip.EquipId)
			{
				flag = true;
				break;
			}
		}
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	private void OnTouch(ClickEvent evt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (m_spriteOk.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			m_wndParent.selectList.Remove(m_equip);
		}
		else
		{
			m_wndParent.selectList.Add(m_equip);
		}
		m_wndParent.RefreshSelectList();
	}
}
