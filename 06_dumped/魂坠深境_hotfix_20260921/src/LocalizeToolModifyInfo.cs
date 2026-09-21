using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizeToolModifyInfo : MonoBehaviour
{
	[SerializeField]
	public GameObject prefab;

	[SerializeField]
	public List<TMPLocalize> added;

	[SerializeField]
	public List<TMPLocalize> modified;

	[SerializeField]
	public List<TextMeshProUGUI> size_modified;
}
