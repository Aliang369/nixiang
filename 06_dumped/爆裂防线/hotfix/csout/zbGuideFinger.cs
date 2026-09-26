using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGuideFinger : zbBaseWnd
{
	private VisualElement m_nodeFinger;

	private VisualElement watchNode;

	private VisualElement targetNode;

	private const float MOVE_SPEED = 400f;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_nodeFinger = UQueryExtensions.Q<VisualElement>(m_RootElement, "sp_finger", (string)null);
	}

	public void RefreshCustom(VisualElement _watchNode, VisualElement _targetNode = null)
	{
		watchNode = _watchNode;
		targetNode = _targetNode;
		((MonoBehaviour)this).StopAllCoroutines();
		if (targetNode == null)
		{
			((MonoBehaviour)this).StartCoroutine(ActionCoroutine());
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(ActionMoveCoroutine());
		}
	}

	private IEnumerator ActionCoroutine()
	{
		yield return null;
		RefreshDetail();
	}

	private IEnumerator ActionMoveCoroutine()
	{
		yield return null;
		while (true)
		{
			Rect worldBound = watchNode.worldBound;
			Vector2 start = ((Rect)(ref worldBound)).center;
			worldBound = targetNode.worldBound;
			Vector2 end = ((Rect)(ref worldBound)).center;
			SetFingerRotation(start, end);
			float num = Vector2.Distance(start, end);
			float duration = num / 400f;
			float time = 0f;
			while (time < duration)
			{
				time += Time.deltaTime;
				float num2 = Mathf.Clamp01(time / duration);
				Vector2 fingerPosition = Vector2.Lerp(start, end, num2);
				SetFingerPosition(fingerPosition);
				yield return null;
			}
			yield return (object)new WaitForSeconds(0.4f);
		}
	}

	private void SetFingerRotation(Vector2 start, Vector2 end)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (end.x - start.x > 0f)
		{
			m_nodeFinger.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		}
		else
		{
			m_nodeFinger.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
		}
	}

	private void SetFingerPosition(Vector2 worldPos)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 0f;
		float num3;
		if ((float)Screen.height / (float)Screen.width < 2f)
		{
			num = ((float)Screen.width - (float)Screen.height / 1480f * 720f) / 2f;
			num3 = (float)Screen.height / 1480f;
		}
		else
		{
			num2 = ((float)Screen.height - (float)Screen.width / 720f * 1480f) / 2f;
			num3 = (float)Screen.width / 720f;
		}
		float num4 = worldPos.x - num / num3;
		float num5 = worldPos.y - num2 / num3 + 100f;
		m_nodeFinger.transform.position = Vector2.op_Implicit(new Vector2(num4, num5));
	}

	private void RefreshDetail()
	{
		zbUtil.SetNodePosition(m_nodeFinger, watchNode);
	}

	protected override void OnHide()
	{
		base.OnHide();
	}
}
