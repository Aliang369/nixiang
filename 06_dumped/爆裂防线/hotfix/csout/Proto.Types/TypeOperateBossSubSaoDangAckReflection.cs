using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateBossSubSaoDangAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateBossSubSaoDangAckReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLk9wZXJhdGVCb3NzU3ViU2FvRGFuZ0Fjay5wcm90bxIFdHlwZXMa" + "FnR5cGUuQXdhcmREZXRhaWwucHJvdG8aHHR5cGUuQm9zc1N1YmplY3REZXRh" + "aWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90byKqAQoYT3BlcmF0ZUJv" + "c3NTdWJTYW9EYW5nQWNrEjEKD0Jvc3NTdWJqZWN0TGlzdBgBIAMoCzIYLnR5" + "cGVzLkJvc3NTdWJqZWN0RGV0YWlsEiUKCUF3YXJkTGlzdBgCIAMoCzISLnR5" + "cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYAyABKAsyES50eXBlcy5E" + "YXRhQ2hhbmdlEg0KBVNjb3JlGAQgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBossSubjectDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateBossSubSaoDangAck), (MessageParser)(object)OperateBossSubSaoDangAck.Parser, new string[4] { "BossSubjectList", "AwardList", "DataChange", "Score" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
