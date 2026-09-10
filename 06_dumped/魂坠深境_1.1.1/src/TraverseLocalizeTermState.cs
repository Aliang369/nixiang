using System.Collections.Generic;
using UnityEngine;

public class TraverseLocalizeTermState
{
	public enum EType
	{
		Export = 1,
		Import = 2,
		Translate = 3,
		ArtCatalog = 4,
		Others = 10
	}

	public List<LocalizeTerm> terms;

	public List<TraverseLocalizeTermBinding> objectFields;

	public HashSet<object> visitedObjects;

	public string currentCategory;

	public EType type;

	public Object lastVisitUnityObject;

	public TraverseLocalizeTermState(EType type)
	{
		this.type = type;
		terms = new List<LocalizeTerm>();
		objectFields = new List<TraverseLocalizeTermBinding>();
		visitedObjects = new HashSet<object>();
	}
}
