using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateSnowVeilPropBuyAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateSnowVeilPropBuyAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLk9wZXJhdGVTbm93VmVpbFByb3BCdXlBY2sucHJvdG8SBXR5cGVz" + "GhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJv" + "dG8aGXR5cGUuU25vd1ZlaWxEZXRhaWwucHJvdG8ilgEKGU9wZXJhdGVTbm93" + "VmVpbFByb3BCdXlBY2sSKwoMU25vd1ZlaWxMaXN0GAEgAygLMhUudHlwZXMu" + "U25vd1ZlaWxEZXRhaWwSJQoJQXdhcmRMaXN0GAIgAygLMhIudHlwZXMuQXdh" + "cmREZXRhaWwSJQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRhdGFDaGFu" + "Z2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9c" + "VHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeSnowVeilDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateSnowVeilPropBuyAck), (MessageParser)(object)OperateSnowVeilPropBuyAck.Parser, new string[3] { "SnowVeilList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
