using System.Collections.Generic;
using UnityEngine;

public class HexShape
{
	[SaveField(null, null)]
	public List<Vector2Int> points = new List<Vector2Int>();

	[SaveField(null, null)]
	public Vector2Int offset;
}
