using UnityEngine;

public struct InfoBoxText
{
	public Color backGroundColor;

	public float fontSize;

	public bool sideText;

	public string text;

	public Color textColor;

	public InfoBoxText(string text, float fontSize, bool sideText = false)
	{
		this.text = text;
		this.fontSize = fontSize;
		textColor = Color.white;
		backGroundColor = Color.white;
		this.sideText = sideText;
	}

	public InfoBoxText(string text, float fontSize, Color textColor, Color backGroundColor, bool sideText = false)
	{
		this.text = text;
		this.fontSize = fontSize;
		this.textColor = textColor;
		this.backGroundColor = backGroundColor;
		this.sideText = sideText;
	}
}
