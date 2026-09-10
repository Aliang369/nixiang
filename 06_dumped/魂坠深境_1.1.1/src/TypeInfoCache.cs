using System;
using System.Reflection;

public class TypeInfoCache
{
	public Type type;

	public bool isDerivedFromGenericParent;

	public FieldInfo indexField;

	public int index;

	public TypeInfoCache(Type type)
	{
		this.type = type;
		isDerivedFromGenericParent = type.BaseType.IsDerivedFromGenericParent(typeof(AssetPreData<>));
		indexField = type.GetField("index", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
	}

	public int GetIndex(ICTAsset target)
	{
		return (int)indexField?.GetValue(target);
	}
}
