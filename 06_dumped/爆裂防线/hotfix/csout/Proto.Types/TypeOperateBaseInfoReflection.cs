using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeOperateBaseInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateBaseInfoReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLk9wZXJhdGVCYXNlSW5mby5wcm90bxIFdHlwZXMaG2VudW0uRU9w" + "ZXJhdGVQbGFuVHlwZS5wcm90byKhAQoPT3BlcmF0ZUJhc2VJbmZvEgwKBFVV" + "SUQYASABKAkSDgoGUGxhbklkGAIgASgFEjAKBFR5cGUYAyABKA4yIi5FT3Bl" + "cmF0ZVBsYW5UeXBlLkVPcGVyYXRlUGxhblR5cGUSDQoFVGl0bGUYBCABKAkS" + "DwoHQ29udGVudBgFIAEoCRIPCgdTdGFydFRtGAYgASgDEg0KBUVuZFRtGAcg" + "ASgDQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEOperatePlanTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateBaseInfo), (MessageParser)(object)OperateBaseInfo.Parser, new string[7] { "UUID", "PlanId", "Type", "Title", "Content", "StartTm", "EndTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
