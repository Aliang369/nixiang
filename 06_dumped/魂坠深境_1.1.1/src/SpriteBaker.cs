using UnityEngine;

public class SpriteBaker : MonoBehaviour
{
	private const string _Add = "_Add";

	private const string _Mul = "_Mul";

	public Material realTimeMat;

	[ColorUsage(true, true)]
	public Color addColor;

	[ColorUsage(true, true)]
	public Color mulColor;

	private SpriteRenderer SprRender => GetComponent<SpriteRenderer>();
}
