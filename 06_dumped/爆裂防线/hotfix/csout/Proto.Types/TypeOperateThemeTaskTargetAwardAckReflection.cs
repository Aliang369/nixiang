using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateThemeTaskTargetAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateThemeTaskTargetAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cil0eXBlLk9wZXJhdGVUaGVtZVRhc2tUYXJnZXRBd2FyZEFjay5wcm90bxIF" + "dHlwZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5n" + "ZS5wcm90bxoadHlwZS5UaGVtZVRhc2tEZXRhaWwucHJvdG8inQEKHk9wZXJh" + "dGVUaGVtZVRhc2tUYXJnZXRBd2FyZEFjaxItCg1UaGVtZVRhc2tMaXN0GAEg" + "AygLMhYudHlwZXMuVGhlbWVUYXNrRGV0YWlsEiUKCUF3YXJkTGlzdBgCIAMo" + "CzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYAyABKAsyES50" + "eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeThemeTaskDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateThemeTaskTargetAwardAck), (MessageParser)(object)OperateThemeTaskTargetAwardAck.Parser, new string[3] { "ThemeTaskList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
