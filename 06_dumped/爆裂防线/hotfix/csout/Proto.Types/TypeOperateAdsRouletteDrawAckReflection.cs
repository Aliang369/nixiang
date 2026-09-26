using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateAdsRouletteDrawAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateAdsRouletteDrawAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLk9wZXJhdGVBZHNSb3VsZXR0ZURyYXdBY2sucHJvdG8SBXR5cGVz" + "Ghx0eXBlLkFkc1JvdWxldHRlRGV0YWlsLnByb3RvGhZ0eXBlLkF3YXJkRGV0" + "YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8inAEKGU9wZXJhdGVB" + "ZHNSb3VsZXR0ZURyYXdBY2sSMQoPQWRzUm91bGV0dGVMaXN0GAEgAygLMhgu" + "dHlwZXMuQWRzUm91bGV0dGVEZXRhaWwSJQoJQXdhcmRMaXN0GAIgAygLMhIu" + "dHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVz" + "LkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdh" + "bWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAdsRouletteDetailReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateAdsRouletteDrawAck), (MessageParser)(object)OperateAdsRouletteDrawAck.Parser, new string[3] { "AdsRouletteList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
