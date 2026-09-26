using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateDailyActiveAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateDailyActiveAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLk9wZXJhdGVEYWlseUFjdGl2ZUFjay5wcm90bxIFdHlwZXMaFnR5" + "cGUuQXdhcmREZXRhaWwucHJvdG8aHHR5cGUuRGFpbHlBY3RpdmVEZXRhaWwu" + "cHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90byKYAQoVT3BlcmF0ZURhaWx5" + "QWN0aXZlQWNrEjEKD0RhaWx5QWN0aXZlTGlzdBgBIAMoCzIYLnR5cGVzLkRh" + "aWx5QWN0aXZlRGV0YWlsEiUKCUF3YXJkTGlzdBgCIAMoCzISLnR5cGVzLkF3" + "YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYAyABKAsyES50eXBlcy5EYXRhQ2hh" + "bmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDailyActiveDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateDailyActiveAck), (MessageParser)(object)OperateDailyActiveAck.Parser, new string[3] { "DailyActiveList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
