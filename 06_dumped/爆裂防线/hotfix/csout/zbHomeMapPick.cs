using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHomeMapPick
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
		Collider2D[] array = Physics2D.OverlapPointAll(new Vector2(val.x, val.y));
		foreach (Collider2D val2 in array)
		{
			foreach (zbFloor floor in zbHomeMgr.Instance.floorMgr.floors)
			{
				if ((Object)(object)val2 == (Object)(object)floor.m_bgCol)
				{
					if (zbSelf.Instance.homelandHandler.GetFloorState(floor.floorIndex) == EActiveState.Active)
					{
						zbWsMsgHandler.Instance.HomelandV2_FloorUnlock_Req(floor.floorIndex);
						return true;
					}
					if (zbSelf.Instance.homelandHandler.GetFloorState(floor.floorIndex) == EActiveState.Pending)
					{
						zbUIMgr.Instance.ShowTask(0);
						return true;
					}
				}
			}
		}
		return false;
	}

	public static zbWorker PickWorker(PointerUpEvent evt)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.homeEditHandler.createRoomRecord != null)
		{
			return null;
		}
		Vector3 val = Vector2.op_Implicit(zbUtil.GetWorldPosByToolKitPosition(((EventBase)evt).target, Vector2.op_Implicit(((PointerEventBase<PointerUpEvent>)(object)evt).localPosition), zbCameraHelper.Instance.mainCamera));
		Collider2D[] array = Physics2D.OverlapPointAll(new Vector2(val.x, val.y));
		for (int i = 0; i < array.Length; i++)
		{
			zbWorker component = ((Component)array[i]).gameObject.GetComponent<zbWorker>();
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
		}
		return null;
	}

	public static zbRoom PickRoom(PointerUpEvent evt)
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
			zbRoom component = ((Component)((Component)array[i]).transform.parent.parent).gameObject.GetComponent<zbRoom>();
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
		}
		return null;
	}

	public static zbRoomEmpty PickRoomCreate(PointerUpEvent evt)
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
			zbRoomEmpty component = ((Component)((Component)array[i]).transform.parent).gameObject.GetComponent<zbRoomEmpty>();
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
		}
		return null;
	}

	public static bool PickRoomHeroTrial(PointerUpEvent evt)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(zbUtil.GetWorldPosByToolKitPosition(((EventBase)evt).target, Vector2.op_Implicit(((PointerEventBase<PointerUpEvent>)(object)evt).localPosition), zbCameraHelper.Instance.mainCamera));
		val.z = 0f;
		Collider2D[] array = Physics2D.OverlapPointAll(Vector2.op_Implicit(val));
		for (int i = 0; i < array.Length; i++)
		{
			if (!((Object)(object)((Component)array[i]).gameObject.GetComponentInParent<zbRoomHero>() == (Object)null))
			{
				zbUIMgr.Instance.ShowHeroTrial();
				return true;
			}
		}
		return false;
	}

	public static zbMergeLeft PickMergeLeft(PointerUpEvent evt)
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
			zbMergeLeft componentInParent = ((Component)array[i]).GetComponentInParent<zbMergeLeft>();
			if ((Object)(object)componentInParent != (Object)null)
			{
				return componentInParent;
			}
		}
		return null;
	}

	public static zbMergeRight PickMergeRight(PointerUpEvent evt)
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
			zbMergeRight componentInParent = ((Component)array[i]).GetComponentInParent<zbMergeRight>();
			if ((Object)(object)componentInParent != (Object)null)
			{
				return componentInParent;
			}
		}
		return null;
	}

	public static zbWall PickWall(PointerUpEvent evt)
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
			zbWall componentInParent = ((Component)array[i]).GetComponentInParent<zbWall>();
			if ((Object)(object)componentInParent != (Object)null)
			{
				return componentInParent;
			}
		}
		return null;
	}
}
