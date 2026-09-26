using System.Collections;
using UnityEngine;

public class zbDungeonMapCinema : MonoBehaviour
{
	private Transform targetTransform;

	public void ApplyPan(Vector3 position)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		float z = ((Component)zbCameraHelper.Instance.mainCamera).transform.position.z;
		Vector3 position2 = default(Vector3);
		((Vector3)(ref position2))._002Ector(position.x, position.y, z);
		((Component)zbCameraHelper.Instance.mainCamera).transform.position = position2;
	}

	private void Update()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetTransform != (Object)null)
		{
			ApplyPan(targetTransform.position);
		}
	}

	public void SmoothTransitionToTarget(Transform newTarget, float duration)
	{
		if (!((Object)(object)newTarget == (Object)null))
		{
			targetTransform = null;
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(SmoothMoveToTarget(newTarget, duration));
		}
	}

	private IEnumerator SmoothMoveToTarget(Transform newTarget, float duration)
	{
		Vector3 startPos = ((Component)zbCameraHelper.Instance.mainCamera).transform.position;
		Vector3 endPos = new Vector3(newTarget.position.x, newTarget.position.y, startPos.z);
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			Vector3 position = Vector3.Lerp(startPos, endPos, num);
			position.z = startPos.z;
			((Component)zbCameraHelper.Instance.mainCamera).transform.position = position;
			yield return null;
		}
		SetFollowTarget(newTarget);
	}

	public void SetFollowTarget(Transform target)
	{
		if (!((Object)(object)target == (Object)null))
		{
			targetTransform = target;
		}
	}

	public Vector2 GetCameraPosition()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.op_Implicit(((Component)zbCameraHelper.Instance.mainCamera).transform.position);
	}
}
