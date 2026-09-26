using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBarDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBarDetailReflection()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChR0eXBlLkJhckRldGFpbC5wcm90bxIFdHlwZXMaE2VudW0uRUJhclR5cGUu" + "cHJvdG8aFWVudW0uRUJvb2xTdGF0ZS5wcm90bxoZdHlwZS5CYXJUYXJnZXRB" + "d2FyZC5wcm90byKsAQoJQmFyRGV0YWlsEiMKB0JhclR5cGUYASABKA4yEi5F" + "QmFyVHlwZS5FQmFyVHlwZRIPCgdSYXJlSGl0GAIgASgFEhIKClRvdGFsQ291" + "bnQYAyABKAUSKQoJRGFpbHlGcmVlGAQgASgOMhYuRUJvb2xTdGF0ZS5FQm9v" + "bFN0YXRlEioKC1RhcmdldEF3YXJkGAUgASgLMhUudHlwZXMuQmFyVGFyZ2V0" + "QXdhcmRCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJv" + "dG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEBarTypeReflection.Descriptor,
			EnumEBoolStateReflection.Descriptor,
			TypeBarTargetAwardReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BarDetail), (MessageParser)(object)BarDetail.Parser, new string[5] { "BarType", "RareHit", "TotalCount", "DailyFree", "TargetAward" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
