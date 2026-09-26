using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSnowVeilRollingEndAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSnowVeilRollingEndAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLk9wZXJhdGVTbm93VmVpbFJvbGxpbmdFbmRBY2sucHJvdG8SBXR5" + "cGVzGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2Uu" + "cHJvdG8aGXR5cGUuU25vd1ZlaWxEZXRhaWwucHJvdG8imQEKHE9wZXJhdGVT" + "bm93VmVpbFJvbGxpbmdFbmRBY2sSKwoMU25vd1ZlaWxMaXN0GAEgAygLMhUu" + "dHlwZXMuU25vd1ZlaWxEZXRhaWwSJQoJQXdhcmRMaXN0GAIgAygLMhIudHlw" + "ZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRh" + "dGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeSnowVeilDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSnowVeilRollingEndAck), (MessageParser)(object)OperateSnowVeilRollingEndAck.Parser, new string[3] { "SnowVeilList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
