using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class DraftPivot : SerializedMonoBehaviour
{
	[Serializable]
	public struct PivotConnection
	{
		public DraftPivot target;

		public List<Vector3> controlPoints;

		public bool IsValid()
		{
			return target != null;
		}

		public List<Vector3> GetAllControlPoints(DraftPivot source)
		{
			if (target == null)
			{
				return new List<Vector3>();
			}
			List<Vector3> list = new List<Vector3>();
			list.Add(source.transform.localPosition);
			if (controlPoints != null)
			{
				foreach (Vector3 controlPoint in controlPoints)
				{
					list.Add(source.transform.localPosition + controlPoint);
				}
			}
			Vector3 item = source.transform.parent.worldToLocalMatrix.MultiplyPoint(target.transform.position);
			list.Add(item);
			Vector3 vector = (list[1] - list[0]).normalized * ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig.connectionLineRadius;
			list[0] += vector;
			Vector3 vector2 = (list[list.Count - 2] - list[list.Count - 1]).normalized * (ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig.connectionLineRadius + ScriptableObjectSingleton<VFXDataPreset>.instance.dungeonMapDisplayConfig.connectionLineArrowLength);
			list[list.Count - 1] += vector2;
			return list;
		}
	}

	public enum PivotType
	{
		Random,
		SectionStart,
		Boss,
		SectionEnd,
		Heal,
		Custom
	}

	public List<PivotConnection> pivotConnections;

	public List<RoomRef> randomRoomRefs;

	public int localProgress;

	public PivotType type;

	public DraftLocationModifier beforeModifier;

	public DraftLocationModifier afterModifier;

	public EnvironmentInfluenceDef presetEnvironmentInfluence;

	public InfluenceDef presetDisorderCriticalInfluence;

	public string exploreGlobalVariable;

	public List<PivotConnection> ValidConnections => pivotConnections.FindAll((PivotConnection p) => p.IsValid());

	public bool ShouldGenerate()
	{
		return true;
	}

	public bool ShouldShow(ApplicationStateInfo applicationState)
	{
		if (!exploreGlobalVariable.IsNullOrEmpty())
		{
			return applicationState.globalVariableManager.GetVariableFlag(exploreGlobalVariable);
		}
		return true;
	}

	private static List<string> GetAllGlobalVariableIds()
	{
		return ApplicationGlobalVariableManager.GetAllGlobalVariableSetups().ConvertAll((GlobalVariableSetup g) => g.id);
	}
}
