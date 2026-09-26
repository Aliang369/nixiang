using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypePrayLanternDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypePrayLanternDetailReflection()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlByYXlMYW50ZXJuRGV0YWlsLnByb3RvEgV0eXBlcxoVZW51bS5F" + "Qm9vbFN0YXRlLnByb3RvGhp0eXBlLk9wZXJhdGVCYXNlSW5mby5wcm90bxoe" + "dHlwZS5PcGVyYXRlVGFyZ2V0RGV0YWlsLnByb3RvGiR0eXBlLlByYXlMYW50" + "ZXJuVWx0aW1hdGVEZXRhaWwucHJvdG8i8wEKEVByYXlMYW50ZXJuRGV0YWls" + "EigKCEJhc2VJbmZvGAEgASgLMhYudHlwZXMuT3BlcmF0ZUJhc2VJbmZvEikK" + "CURhaWx5RnJlZRgCIAEoDjIWLkVCb29sU3RhdGUuRUJvb2xTdGF0ZRIRCglE" + "cmF3Q291bnQYAyABKAUSEgoKVW5sb2NrTGlzdBgEIAMoBRIuCgpUYXJnZXRM" + "aXN0GAUgAygLMhoudHlwZXMuT3BlcmF0ZVRhcmdldERldGFpbBIyCghVbHRp" + "bWF0ZRgGIAEoCzIgLnR5cGVzLlByYXlMYW50ZXJuVWx0aW1hdGVEZXRhaWxC" + "LloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlw" + "ZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			EnumEBoolStateReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateTargetDetailReflection.Descriptor,
			TypePrayLanternUltimateDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(PrayLanternDetail), (MessageParser)(object)PrayLanternDetail.Parser, new string[6] { "BaseInfo", "DailyFree", "DrawCount", "UnlockList", "TargetList", "Ultimate" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
