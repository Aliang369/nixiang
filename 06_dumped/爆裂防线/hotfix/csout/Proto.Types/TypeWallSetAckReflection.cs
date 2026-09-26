using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWallSetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWallSetAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLldhbGxTZXRBY2sucHJvdG8SBXR5cGVzGhZ0eXBlLkRlZmVuc2VJ" + "bmZvLnByb3RvGhN0eXBlLldhbGxJbmZvLnByb3RvIlgKCldhbGxTZXRBY2sS" + "IQoIV2FsbEluZm8YASABKAsyDy50eXBlcy5XYWxsSW5mbxInCgtEZWZlbnNl" + "SW5mbxgCIAEoCzISLnR5cGVzLkRlZmVuc2VJbmZvQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeDefenseInfoReflection.Descriptor,
			TypeWallInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WallSetAck), (MessageParser)(object)WallSetAck.Parser, new string[2] { "WallInfo", "DefenseInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
