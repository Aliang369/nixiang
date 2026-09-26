using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBagItem : zbViewBaseItem
{
	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private VisualElement m_spriteFrag;

	private VisualElement m_spritePropOperate;

	private Label m_lbCount;

	private Button m_btnTouch;

	private VisualElement m_notify;

	private zbBagWnd m_wndParent;

	private zbItemData m_data;

	public zbBagItem(zbBagWnd wndParent)
	{
		m_wndParent = wndParent;
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/bag/tb_bag.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
		m_spriteFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon", (string)null);
		m_spriteFrag = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "frag", (string)null);
		m_spritePropOperate = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "propOperate", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lbCount", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "touch", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnTouch, null, 1f, scaleAnim: false);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(zbItemData data)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		string resourceFrameTex = zbResourceHelper.GetResourceFrameTex(zbResourceHelper.GetResourceQuality(m_data.type, m_data.id));
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(resourceFrameTex);
		if ((Object)(object)val != (Object)null)
		{
			m_spriteFrame.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			m_spriteFrame.MarkDirtyRepaint();
		}
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(m_data.type, m_data.id);
		Texture2D val2 = zbAssetLoader.Instance.LoadResouce<Texture2D>(resorceIconPath.strIconPath);
		if ((Object)(object)val2 != (Object)null)
		{
			m_spriteIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val2));
			m_spriteIcon.MarkDirtyRepaint();
		}
		((TextElement)m_lbCount).text = zbUtil.ConvertToTBMK(m_data.count);
		m_spriteFrag.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_spritePropOperate.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (m_data.type == EResourceType.Prop)
		{
			zbPropCsvRecord recordByKey = zbDataConfig.Instance.propCsvTable.GetRecordByKey((int)m_data.id);
			if (recordByKey != null && recordByKey.type == 16)
			{
				m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			}
			m_spritePropOperate.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((recordByKey == null || recordByKey.getWay != 1) ? 1 : 0));
		}
		else if (m_data.type == EResourceType.Box)
		{
			m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.itemHandler.HasBoxNotify((int)m_data.id)));
		}
	}

	public void OnTouch(ClickEvent evt)
	{
		if ((Object)(object)m_wndParent != (Object)null && m_data != null)
		{
			zbUIMgr.Instance.ShowBagItemDetail(m_data);
		}
	}
}
