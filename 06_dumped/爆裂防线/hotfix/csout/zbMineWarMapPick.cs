using UnityEngine;
using UnityEngine.UIElements;

public class zbMineWarMapPick
{
	public static bool IsPickedFloorUnlock(PointerUpEvent evt)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(zbUtil.GetWorldPosByToolKitPosition(((EventBase)evt).target, Vector2.op_Implicit(((PointerEventBase<PointerUpEvent>)(object)evt).localPosition), zbCameraHelper.Instance.mainCamera));
		Physics2D.OverlapPointAll(new Vector2(val.x, val.y));
		return false;
	}

	public static zbMineWarPoint PickRoom(PointerUpEvent evt)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(zbUtil.GetWorldPosByToolKitPosition(((EventBase)evt).target, Vector2.op_Implicit(((PointerEventBase<PointerUpEvent>)(object)evt).localPosition), zbCameraHelper.Instance.mainCamera));
		Collider2D[] array = Physics2D.OverlapPointAll(new Vector2(val.x, val.y));
		for (int i = 0; i < array.Length; i++)
		{
			zbMineWarPoint componentInParent = ((Component)array[i]).GetComponentInParent<zbMineWarPoint>();
			if ((Object)(object)componentInParent != (Object)null)
			{
				return componentInParent;
			}
		}
		return null;
	}
}
