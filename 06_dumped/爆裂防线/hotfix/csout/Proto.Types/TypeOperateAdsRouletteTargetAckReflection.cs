using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateAdsRouletteTargetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateAdsRouletteTargetAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiZ0eXBlLk9wZXJhdGVBZHNSb3VsZXR0ZVRhcmdldEFjay5wcm90bxIFdHlw" + "ZXMaHHR5cGUuQWRzUm91bGV0dGVEZXRhaWwucHJvdG8aFnR5cGUuQXdhcmRE" + "ZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90byKeAQobT3BlcmF0" + "ZUFkc1JvdWxldHRlVGFyZ2V0QWNrEjEKD0Fkc1JvdWxldHRlTGlzdBgBIAMo" + "CzIYLnR5cGVzLkFkc1JvdWxldHRlRGV0YWlsEiUKCUF3YXJkTGlzdBgCIAMo" + "CzISLnR5cGVzLkF3YXJkRGV0YWlsEiUKCkRhdGFDaGFuZ2UYAyABKAsyES50" + "eXBlcy5EYXRhQ2hhbmdlQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAdsRouletteDetailReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateAdsRouletteTargetAck), (MessageParser)(object)OperateAdsRouletteTargetAck.Parser, new string[3] { "AdsRouletteList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
