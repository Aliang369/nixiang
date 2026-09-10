using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoBoxDisplay : MonoBehaviour
{
	public GameObject bottomDecoration;

	public Transform infoArea;

	public GameObject infoBoxPrefab;

	public List<GameObject> infoBoxs;

	public GameObject TopBox;

	public GameObject middleInfoBoxPrefab => infoBoxPrefab;

	public void SetTextInfo(List<InfoBoxText> infos)
	{
		BuildInfoBoxs(infos.Count);
		for (int i = 0; i < infos.Count; i++)
		{
			TextMeshProUGUI componentInChildren = infoBoxs[i].GetComponentInChildren<TextMeshProUGUI>();
			Image componentInChildren2 = infoBoxs[i].GetComponentInChildren<Image>();
			componentInChildren.text = infos[i].text;
			componentInChildren.fontSize = infos[i].fontSize;
			componentInChildren.color = infos[i].textColor;
			componentInChildren2.color = infos[i].backGroundColor;
		}
	}

	private void BuildInfoBoxs(int infoCount)
	{
		while (infoBoxs.Count < infoCount)
		{
			GameObject gameObject = middleInfoBoxPrefab.InstantiatePrefabIn(infoArea);
			gameObject.transform.SetSiblingIndex(infoBoxs[infoBoxs.Count - 1].transform.GetSiblingIndex() + 1);
			infoBoxs.Add(gameObject);
		}
		for (int i = 0; i < infoBoxs.Count; i++)
		{
			if (i < infoCount)
			{
				infoBoxs[i].SetActive(value: true);
			}
			else
			{
				infoBoxs[i].SetActive(value: false);
			}
		}
	}
}
