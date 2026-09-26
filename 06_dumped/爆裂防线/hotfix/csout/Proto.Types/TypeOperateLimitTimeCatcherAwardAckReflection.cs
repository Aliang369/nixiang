using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateLimitTimeCatcherAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateLimitTimeCatcherAwardAckReflection()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cip0eXBlLk9wZXJhdGVMaW1pdFRpbWVDYXRjaGVyQXdhcmRBY2sucHJvdG8S" + "BXR5cGVzGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFu" + "Z2UucHJvdG8aIXR5cGUuTGltaXRUaW1lQ2F0Y2hlckRldGFpbC5wcm90byKs" + "AQofT3BlcmF0ZUxpbWl0VGltZUNhdGNoZXJBd2FyZEFjaxI7ChRMaW1pdFRp" + "bWVDYXRjaGVyTGlzdBgBIAMoCzIdLnR5cGVzLkxpbWl0VGltZUNhdGNoZXJE" + "ZXRhaWwSJQoJQXdhcmRMaXN0GAIgAygLMhIudHlwZXMuQXdhcmREZXRhaWwS" + "JQoKRGF0YUNoYW5nZRgDIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87" + "cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnBy" + "b3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeLimitTimeCatcherDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateLimitTimeCatcherAwardAck), (MessageParser)(object)OperateLimitTimeCatcherAwardAck.Parser, new string[3] { "LimitTimeCatcherList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
