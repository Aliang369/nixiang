using System.Collections.Generic;
using UnityEngine;

public class InfoBubble : MonoBehaviour
{
	public GameObject bottomDecoration;

	public Transform infoArea;

	public GameObject infoBoxPrefab;

	public List<GameObject> infoBoxs;

	public InfoBoxDisplay mainBox;

	public InfoBoxDisplay sideBox;

	public GameObject topIngoBox;

	private GameStateInfo gameState;

	public GameObject middleInfoBoxPrefab => infoBoxPrefab;

	public void SetShowInfo(InfoBoxInfo info, GameStateInfo gameState)
	{
		if (info != null)
		{
			this.gameState = gameState;
			base.gameObject.SetActive(value: true);
			base.transform.position = gameState.applicationState.currentVirtualCamera.bindedCamera.finalCamera.ScreenToWorldPoint(info.screenPos);
			base.transform.localPosition += GetInfoOffset(info.screenPos, info.withTouchOffset);
			if (info.mainTexts != null && info.mainTexts.Count > 0)
			{
				mainBox.SetTextInfo(info.mainTexts);
				mainBox.gameObject.SetActive(value: true);
			}
			else
			{
				mainBox.gameObject.SetActive(value: false);
			}
			if (info.mainTexts != null && info.mainTexts.Count > 0)
			{
				sideBox.gameObject.SetActive(value: true);
				sideBox.SetTextInfo(info.sideTexts);
				UpdateSideBoxLocalPos();
			}
			else
			{
				sideBox.gameObject.SetActive(value: false);
			}
		}
	}

	public void UpdateSideBoxLocalPos()
	{
		Vector3 vector = gameState.applicationState.currentVirtualCamera.bindedCamera.finalCamera.WorldToScreenPoint(mainBox.transform.position);
		Rect rect = mainBox.GetComponent<RectTransform>().rect;
		Rect rect2 = sideBox.GetComponent<RectTransform>().rect;
		if (vector.x + rect.xMax + rect2.width < Screen.safeArea.width + Screen.safeArea.x)
		{
			float num = rect.xMax - rect.center.x + rect2.center.x - rect2.xMin;
			sideBox.transform.localPosition = mainBox.transform.localPosition + Vector3.right * num;
		}
		else
		{
			float num2 = rect.center.x - rect.xMin + rect2.xMax - rect2.center.x;
			sideBox.transform.localPosition = mainBox.transform.localPosition - Vector3.right * num2;
		}
	}

	private Vector3 GetInfoOffset(Vector3 screenPos, bool withTouchOffset)
	{
		Vector2 size = infoArea.GetComponent<RectTransform>().GetSize();
		Vector3 a = 0.5f * size;
		if (withTouchOffset)
		{
			a += new Vector3(0f, 150f, 0f);
		}
		Vector3 b = 0.5f * new Vector3(Screen.safeArea.width + Screen.safeArea.x, Screen.safeArea.height + Screen.safeArea.y, 0f) - screenPos;
		b = new Vector3((b.x > 0f) ? 1 : (-1), 1f, 0f);
		return Vector3.Scale(a, b);
	}
}
