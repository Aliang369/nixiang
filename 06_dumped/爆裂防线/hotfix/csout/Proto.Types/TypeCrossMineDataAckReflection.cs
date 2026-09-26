using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeCrossMineDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossMineDataAckReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkNyb3NzTWluZURhdGFBY2sucHJvdG8SBXR5cGVzGhVlbnVtLkVF" + "cnJvckNvZGUucHJvdG8aGnR5cGUuTWluZVdhckJhc2VJbmZvLnByb3RvGhl0" + "eXBlLk1pbmVXYXJNYXBJbmZvLnByb3RvIrQBChBDcm9zc01pbmVEYXRhQWNr" + "EikKCUVycm9yQ29kZRgBIAEoDjIWLkVFcnJvckNvZGUuRUVycm9yQ29kZRIQ" + "CghTZXJ2ZXJJZBgCIAEoBRIvCg9NaW5lV2FyQmFzZUluZm8YAyABKAsyFi50" + "eXBlcy5NaW5lV2FyQmFzZUluZm8SMgoTQ3Jvc3NNaW5lV2FyTWFwSW5mbxgE" + "IAEoCzIVLnR5cGVzLk1pbmVXYXJNYXBJbmZvQi5aCS4vO3Byb3RvY6oCC1By" + "b3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEErrorCodeReflection.Descriptor,
			TypeMineWarBaseInfoReflection.Descriptor,
			TypeMineWarMapInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossMineDataAck), (MessageParser)(object)CrossMineDataAck.Parser, new string[4] { "ErrorCode", "ServerId", "MineWarBaseInfo", "CrossMineWarMapInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
