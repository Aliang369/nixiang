using System;
using UnityEngine;
using UnityEngine.UI;

public class SkinSlotDisplay : CellUI<Item>
{
	[SerializeField]
	private Image image;

	[NonSerialized]
	private GameObject subObject;

	protected override void OnSetup(Item _)
	{
	}

	public void OverrideSkin(Item item, ItemSkinBaseDef.ItemSkinBaseSlotType type)
	{
		SetSprite(item.GetImageSetup(type));
		SetSubObject(item.GetSubItemSetup(type));
	}

	private void SetSprite(ItemSkinBaseDef.ImageSetup setup)
	{
		if (setup == null)
		{
			image.gameObject.SetActive(value: false);
			return;
		}
		image.gameObject.SetActive(value: true);
		setup.Apply(image);
	}

	private void SetSubObject(ItemSkinBaseDef.SubItemSetup setup)
	{
		if (subObject != null)
		{
			UnityEngine.Object.Destroy(subObject);
		}
		if (!(setup?.subObject == null))
		{
			subObject = UnityEngine.Object.Instantiate(setup.subObject, base.transform);
			subObject.transform.localPosition = Vector3.zero;
			(subObject.transform as RectTransform).SetWidth(base.rectTransform.GetWidth());
			(subObject.transform as RectTransform).SetHeight(base.rectTransform.GetHeight());
		}
	}

	private void OnValidate()
	{
		if (LocalSettings.autoUpdate)
		{
			image = GetComponent<Image>();
		}
	}
}
