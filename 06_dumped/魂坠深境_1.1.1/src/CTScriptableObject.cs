using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

public abstract class CTScriptableObject : SerializedScriptableObject, ICTAsset, IChildren<ICTAsset>
{
	[AttributeUsage(AttributeTargets.Field)]
	protected class CTSOAssetNameAttribute : Attribute
	{
	}

	public string id;

	public bool obsolate;

	public bool developingAssetFlag;

	public int SOindex;

	[TextArea]
	public string developerTip;

	protected static ApplicationStateInfo applicationState => Bootstrap.applicationState;

	int ICTAsset.AssetIndex
	{
		get
		{
			return SOindex;
		}
		set
		{
			SOindex = value;
		}
	}

	string ICTAsset.AssetGuid
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public string FullID => ((ICTAsset)this).GetFullID();

	public string GetPaddingID => ((ICTAsset)this).GetPaddingID();

	public abstract string CTAssetIDPrepend { get; }

	public virtual Type CTAssetType => GetType();

	public static List<Type> allCTSubClasses => (from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly s) => s.GetTypes())
		where t.BaseType == typeof(CTScriptableObject)
		select t).ToList();

	public Type CTSOType
	{
		get
		{
			if (!(GetType().BaseType == typeof(CTScriptableObject)))
			{
				return GetType().GetBaseClasses().First((Type t) => t.BaseType == typeof(CTScriptableObject));
			}
			return GetType();
		}
	}

	public virtual List<ICTAsset> GetChildren()
	{
		return null;
	}

	public bool IsValidAsset()
	{
		if (!obsolate)
		{
			if (CSCommon.developingAssetFilter)
			{
				return !developingAssetFlag;
			}
			return true;
		}
		return false;
	}
}
