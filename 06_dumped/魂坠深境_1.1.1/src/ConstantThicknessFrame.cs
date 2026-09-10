using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ConstantThicknessFrame : MonoBehaviour
{
	public Color frameColor
	{
		get
		{
			return GetComponent<Image>().material.GetColor("_FrameColor");
		}
		set
		{
			GetComponent<Image>().material.SetColor("_FrameColor", value);
		}
	}

	public Color innerColor
	{
		get
		{
			return GetComponent<Image>().material.GetColor("_MainColor");
		}
		set
		{
			GetComponent<Image>().material.SetColor("_MainColor", value);
		}
	}

	public float Thickness
	{
		get
		{
			return GetComponent<Image>().material.GetFloat("_Thickness");
		}
		set
		{
			GetComponent<Image>().material.SetFloat("_Thickness", value);
		}
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			GetComponent<Image>().material = Object.Instantiate(GetComponent<Image>().material);
		}
	}

	private void Update()
	{
		GetComponent<Image>().material.SetVector("_Size", new Vector4(GetComponent<RectTransform>().sizeDelta.x, GetComponent<RectTransform>().sizeDelta.y, 0f, 0f));
	}
}
