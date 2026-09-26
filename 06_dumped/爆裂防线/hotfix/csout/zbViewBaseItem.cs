using UnityEngine;
using UnityEngine.UIElements;

public class zbViewBaseItem : VisualElement
{
	public zbViewBaseItem()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)this)._002Ector();
		((VisualElement)this).style.backgroundColor = new StyleColor(Color.clear);
		((VisualElement)this).ClearClassList();
	}

	public void SetPadding(string position, float px)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		switch (position)
		{
		case "Top":
			((VisualElement)this).style.paddingTop = StyleLength.op_Implicit(px);
			break;
		case "Left":
			((VisualElement)this).style.paddingLeft = StyleLength.op_Implicit(px);
			break;
		case "Right":
			((VisualElement)this).style.paddingRight = StyleLength.op_Implicit(px);
			break;
		case "Bottom":
			((VisualElement)this).style.paddingBottom = StyleLength.op_Implicit(px);
			break;
		}
	}

	public void Hide()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)this).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}
}
