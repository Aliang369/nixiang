using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateAdsZhanLingAdAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateAdsZhanLingAdAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cid0eXBlLk9wZXJhdGVBZHNaaGFuTGluZ0FkQXdhcmRBY2sucHJvdG8SBXR5" + "cGVzGhx0eXBlLkFkc1poYW5MaW5nRGV0YWlsLnByb3RvGhZ0eXBlLkF3YXJk" + "RGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8inwEKHE9wZXJh" + "dGVBZHNaaGFuTGluZ0FkQXdhcmRBY2sSMQoPQWRzWmhhbkxpbmdMaXN0GAEg" + "AygLMhgudHlwZXMuQWRzWmhhbkxpbmdEZXRhaWwSJQoJQXdhcmRMaXN0GAIg" + "AygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoKRGF0YUNoYW5nZRgDIAEoCzIR" + "LnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPK" + "AhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAdsZhanLingDetailReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateAdsZhanLingAdAwardAck), (MessageParser)(object)OperateAdsZhanLingAdAwardAck.Parser, new string[3] { "AdsZhanLingList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
