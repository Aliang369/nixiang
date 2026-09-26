using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateAdsZhanLingFreeAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateAdsZhanLingFreeAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cil0eXBlLk9wZXJhdGVBZHNaaGFuTGluZ0ZyZWVBd2FyZEFjay5wcm90bxIF" + "dHlwZXMaHHR5cGUuQWRzWmhhbkxpbmdEZXRhaWwucHJvdG8aFnR5cGUuQXdh" + "cmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90byKhAQoeT3Bl" + "cmF0ZUFkc1poYW5MaW5nRnJlZUF3YXJkQWNrEjEKD0Fkc1poYW5MaW5nTGlz" + "dBgBIAMoCzIYLnR5cGVzLkFkc1poYW5MaW5nRGV0YWlsEiUKCUF3YXJkTGlz" + "dBgCIAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYAyAB" + "KAsyES50eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAdsZhanLingDetailReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateAdsZhanLingFreeAwardAck), (MessageParser)(object)OperateAdsZhanLingFreeAwardAck.Parser, new string[3] { "AdsZhanLingList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
