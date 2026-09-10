using DG.Tweening;
using Sirenix.OdinInspector;

public class CameraControlData : SerializedScriptableObject
{
	public CameraControlNode node;

	public Sequence Run(ApplicationStateInfo applicationState, float battleScale)
	{
		return node.Run(applicationState, battleScale);
	}
}
