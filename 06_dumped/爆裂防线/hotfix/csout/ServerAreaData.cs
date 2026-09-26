using System.Collections.Generic;
using Proto.Enums;

public class ServerAreaData
{
	public int areaId;

	public int logicX;

	public int logicY;

	public ESeasonForce force;

	public int areaType;

	public bool isCenter;

	public List<ResourceData> resources;
}
