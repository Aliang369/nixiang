using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbEquipSlotTabEquip
{
	private zbEquipSlotWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbMainAttrValue;

	private VisualElement m_nodeSubAttr;

	private VisualElement[] m_nodeSubAttrs = (VisualElement[])(object)new VisualElement[6];

	private Label[] m_lbSubAttrs = (Label[])(object)new Label[6];

	private Label[] m_lbSubAttrValues = (Label[])(object)new Label[6];

	public void OnEnable(zbEquipSlotWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "title_main", (string)null);
		m_lbMainAttrValue = UQueryExtensions.Q<Label>(val, "lb_value", (string)null);
		m_nodeSubAttr = UQueryExtensions.Q<VisualElement>(m_RootElement, "title_sub", (string)null);
		for (int i = 0; i < 6; i++)
		{
			m_nodeSubAttrs[i] = UQueryExtensions.Q<VisualElement>(m_nodeSubAttr, "attr" + i, (string)null);
			m_lbSubAttrs[i] = UQueryExtensions.Q<Label>(m_nodeSubAttrs[i], "lb_attr", (string)null);
			m_lbSubAttrValues[i] = UQueryExtensions.Q<Label>(m_nodeSubAttrs[i], "lb_value", (string)null);
		}
	}

	public void Refresh(EquipSlotDetail data)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if (data == null || data.Equip == null)
		{
			return;
		}
		((TextElement)m_lbMainAttrValue).text = zbSelf.Instance.equipHandler.GetEquipMainAttrValue(data.Equip.EquipTid, data.Equip.Quality).ToString();
		for (int i = 0; i < 6; i++)
		{
			((VisualElement)m_lbSubAttrs[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbSubAttrValues[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (EquipSubAttrDetail subAttr in data.Equip.SubAttrList)
		{
			if (num < m_nodeSubAttrs.Length)
			{
				((VisualElement)m_lbSubAttrs[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbSubAttrValues[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((TextElement)m_lbSubAttrs[num]).text = zbAttrHelper.GetAttrName(subAttr.AttrType, subAttr.FuncId, subAttr.AttrId);
				if (subAttr.ShowPct == EBoolState.False)
				{
					((TextElement)m_lbSubAttrValues[num]).text = (subAttr.Value * (float)data.Equip.SubLevel).ToString("F2");
				}
				else
				{
					((TextElement)m_lbSubAttrValues[num]).text = (subAttr.Value * (float)data.Equip.SubLevel * 100f).ToString("F2") + "%";
				}
			}
			num++;
		}
	}
}
