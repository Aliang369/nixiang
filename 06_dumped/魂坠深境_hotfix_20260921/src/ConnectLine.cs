using UnityEngine;

public class ConnectLine : CellUI<int>
{
	[SerializeField]
	public Transform startTransform;

	[SerializeField]
	public Transform endTransform;

	[SerializeField]
	public float width;

	protected override void OnSetup(int t)
	{
	}

	public void SetConnectLine()
	{
		RectTransform obj = base.transform as RectTransform;
		base.transform.position = endTransform.position;
		Vector3 localPosition = base.transform.localPosition;
		base.transform.position = startTransform.position;
		Vector3 localPosition2 = base.transform.localPosition;
		obj.SetHeight(Vector3.Distance(localPosition2, localPosition));
		obj.rotation = Quaternion.LookRotation(Vector3.forward, localPosition2 - localPosition);
	}
}
