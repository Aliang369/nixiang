using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateLimitTimeCatcherGrabAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateLimitTimeCatcherGrabAckReflection()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cil0eXBlLk9wZXJhdGVMaW1pdFRpbWVDYXRjaGVyR3JhYkFjay5wcm90bxIF" + "dHlwZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5n" + "ZS5wcm90bxohdHlwZS5MaW1pdFRpbWVDYXRjaGVyRGV0YWlsLnByb3RvGid0" + "eXBlLkxpbWl0VGltZUNhdGNoZXJUYXJnZXREZXRhaWwucHJvdG8i4gEKHk9w" + "ZXJhdGVMaW1pdFRpbWVDYXRjaGVyR3JhYkFjaxI7ChRMaW1pdFRpbWVDYXRj" + "aGVyTGlzdBgBIAMoCzIdLnR5cGVzLkxpbWl0VGltZUNhdGNoZXJEZXRhaWwS" + "NQoIRHJvcExpc3QYAiADKAsyIy50eXBlcy5MaW1pdFRpbWVDYXRjaGVyVGFy" + "Z2V0RGV0YWlsEiUKCUF3YXJkTGlzdBgDIAMoCzISLnR5cGVzLkF3YXJkRGV0" + "YWlsEiUKCkRhdGFDaGFuZ2UYBCABKAsyES50eXBlcy5EYXRhQ2hhbmdlQi5a" + "CS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVz" + "YgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeLimitTimeCatcherDetailReflection.Descriptor,
			TypeLimitTimeCatcherTargetDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateLimitTimeCatcherGrabAck), (MessageParser)(object)OperateLimitTimeCatcherGrabAck.Parser, new string[4] { "LimitTimeCatcherList", "DropList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
