namespace UnityEngine.UI;

[AddComponentMenu("Layout/Reverse Horizontal Layout Group")]
public class ReverseHorizontalGroup : HorizontalLayoutGroup
{
	public override void SetLayoutHorizontal()
	{
		RectTransform[] array = new RectTransform[base.rectChildren.Count];
		int count = base.rectChildren.Count;
		int num = count - 1;
		for (int num2 = num; num2 >= 0; num2--)
		{
			array[num - num2] = base.rectChildren[num2];
		}
		for (int i = 0; i < count; i++)
		{
			base.rectChildren[i] = array[i];
		}
		base.SetLayoutHorizontal();
	}
}
