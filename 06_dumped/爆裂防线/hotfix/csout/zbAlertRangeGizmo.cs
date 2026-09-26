using UnityEngine;

public class zbAlertRangeGizmo : MonoBehaviour
{
	private GameObject alert_bg;

	private void Awake()
	{
		alert_bg = ((Component)((Component)this).transform.Find("alert_bg")).gameObject;
		((Component)this).gameObject.SetActive(false);
	}

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void SetAlertHeight(float height)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		alert_bg.transform.localPosition = new Vector3(alert_bg.transform.localPosition.x, height, alert_bg.transform.localPosition.z);
	}
}
