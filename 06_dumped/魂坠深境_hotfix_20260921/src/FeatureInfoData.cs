using System;
using UnityEngine;

public class FeatureInfoData
{
	[SerializeField]
	public CTScriptableObject featureObject;

	[NonSerialized]
	public object target;

	public FeatureInfoData()
	{
	}

	public FeatureInfoData(CTScriptableObject featureObject)
	{
		this.featureObject = featureObject;
	}

	public FeatureInfoData(object target)
	{
		this.target = target;
		featureObject = target as CTScriptableObject;
	}
}
