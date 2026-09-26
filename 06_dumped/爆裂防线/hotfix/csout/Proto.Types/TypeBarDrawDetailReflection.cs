using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeBarDrawDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeBarDrawDetailReflection()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chh0eXBlLkJhckRyYXdEZXRhaWwucHJvdG8SBXR5cGVzGhNlbnVtLkVCYXJU" + "eXBlLnByb3RvGhp0eXBlLk9wZXJhdGVCYXNlSW5mby5wcm90bxoedHlwZS5P" + "cGVyYXRlVGFyZ2V0RGV0YWlsLnByb3RvIp0BCg1CYXJEcmF3RGV0YWlsEigK" + "CEJhc2VJbmZvGAEgASgLMhYudHlwZXMuT3BlcmF0ZUJhc2VJbmZvEi4KClRh" + "cmdldExpc3QYAiADKAsyGi50eXBlcy5PcGVyYXRlVGFyZ2V0RGV0YWlsEiMK" + "B0JhclR5cGUYAyABKA4yEi5FQmFyVHlwZS5FQmFyVHlwZRINCgVDb3VudBgE" + "IAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90" + "b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumEBarTypeReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateTargetDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(BarDrawDetail), (MessageParser)(object)BarDrawDetail.Parser, new string[4] { "BaseInfo", "TargetList", "BarType", "Count" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
