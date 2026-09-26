using UnityEngine;

public class zbSeasonCar : MonoBehaviour
{
	private SpriteRenderer m_renderer;

	public int LineId { get; private set; }

	public void Init(int lineId, Sprite sprite, string sortingLayerName, int sortingOrder)
	{
		LineId = lineId;
		m_renderer = ((Component)this).gameObject.AddComponent<SpriteRenderer>();
		m_renderer.sprite = sprite;
		((Renderer)m_renderer).sortingLayerName = sortingLayerName;
		((Renderer)m_renderer).sortingOrder = sortingOrder;
		((Component)this).gameObject.AddComponent<BoxCollider2D>();
	}

	public void OnClick()
	{
	}

	public void SetSprite(Sprite sprite)
	{
		m_renderer.sprite = sprite;
	}

	public void SetPosition(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = position;
	}

	private void OnDestroy()
	{
	}
}
