using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class CenterScrollContent : MonoBehaviour
{
	public RectTransform viewport;

	private RectTransform _contentRect;

	private ContentSizeFitter _fitter;

	private void Awake()
	{
		_contentRect = GetComponent<RectTransform>();
		_fitter = GetComponent<ContentSizeFitter>();
	}

	private void Update()
	{
		if (!(viewport == null) && !(_fitter == null))
		{
			float preferredHeight = LayoutUtility.GetPreferredHeight(_contentRect);
			float height = viewport.rect.height;
			float num = Mathf.Max(preferredHeight, height);
			if (_contentRect.sizeDelta.y != num)
			{
				_contentRect.sizeDelta = new Vector2(_contentRect.sizeDelta.x, num);
			}
		}
	}
}
