using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAccountRebateAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAccountRebateAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkFjY291bnRSZWJhdGVBd2FyZEFjay5wcm90bxIFdHlwZXMaF3R5" + "cGUuQXV4QXdhcmRJbmZvLnByb3RvGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3Rv" + "GhV0eXBlLkRhdGFDaGFuZ2UucHJvdG8ikAEKFUFjY291bnRSZWJhdGVBd2Fy" + "ZEFjaxIpCgxBdXhBd2FyZEluZm8YASABKAsyEy50eXBlcy5BdXhBd2FyZElu" + "Zm8SJQoJQXdhcmRMaXN0GAIgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoK" + "RGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAuxAwardInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AccountRebateAwardAck), (MessageParser)(object)AccountRebateAwardAck.Parser, new string[3] { "AuxAwardInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
