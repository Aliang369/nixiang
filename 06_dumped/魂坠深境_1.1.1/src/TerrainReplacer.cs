using System.Collections.Generic;
using UnityEngine;

public class TerrainReplacer : MonoBehaviour
{
	[SerializeField]
	private List<TerrainData> datas;

	[SerializeField]
	private Terrain terrain;

	private void Replace(TerrainData terrainData)
	{
		terrain.terrainData = terrainData;
	}

	public void Replace(int i)
	{
		Replace(datas[i]);
	}
}
