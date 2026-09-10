using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonMap
{
	[SaveField(null, null)]
	public PathLocation initLocation;

	[SaveField(null, null)]
	public PathLocation currentLocation;

	[SaveField(null, null)]
	public List<PathLocation> locations;

	[SaveField(null, null)]
	public List<PathConnectionLine> connectionLines;

	[SaveField(null, null)]
	public Vector2 mapSize;

	[SaveField(null, null)]
	public DraftMapRef sourceDraftMap;

	[NonSerialized]
	private DraftMap _cachedDraftMap;

	public PathLocation VisibleEndLocation
	{
		get
		{
			PathLocation pathLocation = locations.FirstOrDefault((PathLocation l) => l.type == PathLocation.LocationType.Boss);
			if (pathLocation != null)
			{
				return pathLocation;
			}
			PathLocation pathLocation2 = locations.FirstOrDefault((PathLocation l) => l.type == PathLocation.LocationType.SectionEnd);
			if (pathLocation2 != null)
			{
				return pathLocation2;
			}
			return locations.MaxBy((PathLocation l) => l.localProgress);
		}
	}

	public DraftMap GetLoadedDraftMap(AssetCatalog catalog)
	{
		if (_cachedDraftMap == null)
		{
			_cachedDraftMap = sourceDraftMap?.GetAssetPreData(catalog)?.GetOrSyncLoadAsset();
		}
		return _cachedDraftMap;
	}

	public int GetVisibleMapProgressLength()
	{
		return VisibleEndLocation.localProgress - initLocation.localProgress + 1;
	}
}
