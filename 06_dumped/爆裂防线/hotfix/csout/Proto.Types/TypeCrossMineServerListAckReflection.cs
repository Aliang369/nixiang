using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossMineServerListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossMineServerListAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkNyb3NzTWluZVNlcnZlckxpc3RBY2sucHJvdG8SBXR5cGVzGiB0" + "eXBlLkNyb3NzTWluZVNlcnZlckRldGFpbC5wcm90bxoadHlwZS5NaW5lV2Fy" + "QmFzZUluZm8ucHJvdG8ihAEKFkNyb3NzTWluZVNlcnZlckxpc3RBY2sSLwoP" + "TWluZVdhckJhc2VJbmZvGAEgASgLMhYudHlwZXMuTWluZVdhckJhc2VJbmZv" + "EjkKE0Nyb3NzTWluZVNlcnZlckxpc3QYAiADKAsyHC50eXBlcy5Dcm9zc01p" + "bmVTZXJ2ZXJEZXRhaWxCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJa" + "YmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeCrossMineServerDetailReflection.Descriptor,
			TypeMineWarBaseInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossMineServerListAck), (MessageParser)(object)CrossMineServerListAck.Parser, new string[2] { "MineWarBaseInfo", "CrossMineServerList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
