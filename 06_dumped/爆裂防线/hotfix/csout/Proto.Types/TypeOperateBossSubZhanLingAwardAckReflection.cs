using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateBossSubZhanLingAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateBossSubZhanLingAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cil0eXBlLk9wZXJhdGVCb3NzU3ViWmhhbkxpbmdBd2FyZEFjay5wcm90bxIF" + "dHlwZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aHHR5cGUuQm9zc1N1Ympl" + "Y3REZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90byKhAQoeT3Bl" + "cmF0ZUJvc3NTdWJaaGFuTGluZ0F3YXJkQWNrEjEKD0Jvc3NTdWJqZWN0TGlz" + "dBgBIAMoCzIYLnR5cGVzLkJvc3NTdWJqZWN0RGV0YWlsEiUKCUF3YXJkTGlz" + "dBgCIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYAyAB" + "KAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBossSubjectDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateBossSubZhanLingAwardAck), (MessageParser)(object)OperateBossSubZhanLingAwardAck.Parser, new string[3] { "BossSubjectList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
