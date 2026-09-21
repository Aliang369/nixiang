using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LayoutSpacingAdjustor : MonoBehaviour
{
	private float baseSpacingX;

	private float baseSpacingY;

	private GridLayoutGroup gridLayoutGroup;

	private HorizontalLayoutGroup horizontalLayoutGroup;

	private VerticalLayoutGroup verticalLayoutGroup;

	private void Start()
	{
		gridLayoutGroup = GetComponent<GridLayoutGroup>();
		if (gridLayoutGroup != null)
		{
			baseSpacingX = gridLayoutGroup.spacing.x;
			baseSpacingY = gridLayoutGroup.spacing.y;
		}
		horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
		if (horizontalLayoutGroup != null)
		{
			baseSpacingX = horizontalLayoutGroup.spacing;
		}
		verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
		if (verticalLayoutGroup != null)
		{
			baseSpacingY = verticalLayoutGroup.spacing;
		}
	}

	public void Adjust()
	{
		float height = GetComponent<RectTransform>().rect.height;
		float width = GetComponent<RectTransform>().rect.width;
		int num = base.gameObject.transform.GetChildren().Where(delegate(Transform i)
		{
			if (i.gameObject.activeInHierarchy)
			{
				LayoutElement component = i.GetComponent<LayoutElement>();
				if ((object)component == null)
				{
					return true;
				}
				return !component.ignoreLayout;
			}
			return false;
		}).Count();
		float num2 = base.gameObject.transform.GetChildren().Where(delegate(Transform i)
		{
			if (i.gameObject.activeInHierarchy)
			{
				LayoutElement component = i.GetComponent<LayoutElement>();
				if ((object)component == null)
				{
					return true;
				}
				return !component.ignoreLayout;
			}
			return false;
		}).Sum((Transform i) => i.GetComponent<RectTransform>().rect.width + baseSpacingX);
		float num3 = base.gameObject.transform.GetChildren().Where(delegate(Transform i)
		{
			if (i.gameObject.activeInHierarchy)
			{
				LayoutElement component = i.GetComponent<LayoutElement>();
				if ((object)component == null)
				{
					return true;
				}
				return !component.ignoreLayout;
			}
			return false;
		}).Sum((Transform i) => i.GetComponent<RectTransform>().rect.height + baseSpacingY);
		if (num2 < width)
		{
			if (gridLayoutGroup != null)
			{
				gridLayoutGroup.spacing = new Vector2(baseSpacingX, gridLayoutGroup.spacing.y);
			}
			if (horizontalLayoutGroup != null)
			{
				horizontalLayoutGroup.spacing = baseSpacingX;
			}
		}
		else
		{
			float num4 = (num2 - width) / (float)num;
			float num5 = baseSpacingX - num4;
			if (gridLayoutGroup != null)
			{
				gridLayoutGroup.spacing = new Vector2(num5, gridLayoutGroup.spacing.y);
			}
			if (horizontalLayoutGroup != null)
			{
				horizontalLayoutGroup.spacing = num5;
			}
		}
		if (num3 < height)
		{
			if (gridLayoutGroup != null)
			{
				gridLayoutGroup.spacing = new Vector2(gridLayoutGroup.spacing.x, baseSpacingY);
			}
			if (verticalLayoutGroup != null)
			{
				verticalLayoutGroup.spacing = baseSpacingY;
			}
			return;
		}
		float num6 = (num3 - height) / (float)num;
		float num7 = baseSpacingY - num6;
		if (gridLayoutGroup != null)
		{
			gridLayoutGroup.spacing = new Vector2(gridLayoutGroup.spacing.x, num7);
		}
		if (verticalLayoutGroup != null)
		{
			verticalLayoutGroup.spacing = num7;
		}
	}
}
