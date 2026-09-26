using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdsRouletteDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdsRouletteDetailReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLkFkc1JvdWxldHRlRGV0YWlsLnByb3RvEgV0eXBlcxoedHlwZS5B" + "ZHNSb3VsZXR0ZUJhc2VEYXRhLnByb3RvGiZ0eXBlLkxpbWl0VGltZVJvdWxl" + "dHRlUmFuZERldGFpbC5wcm90bxoadHlwZS5PcGVyYXRlQmFzZUluZm8ucHJv" + "dG8aHnR5cGUuT3BlcmF0ZVRhcmdldERldGFpbC5wcm90byLRAQoRQWRzUm91" + "bGV0dGVEZXRhaWwSKAoIQmFzZUluZm8YASABKAsyFi50eXBlcy5PcGVyYXRl" + "QmFzZUluZm8SLAoIQmFzZURhdGEYAiABKAsyGi50eXBlcy5BZHNSb3VsZXR0" + "ZUJhc2VEYXRhEjQKCFNsb3RMaXN0GAMgAygLMiIudHlwZXMuTGltaXRUaW1l" + "Um91bGV0dGVSYW5kRGV0YWlsEi4KClRhcmdldExpc3QYBCADKAsyGi50eXBl" + "cy5PcGVyYXRlVGFyZ2V0RGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAdsRouletteBaseDataReflection.Descriptor,
			TypeLimitTimeRouletteRandDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateTargetDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdsRouletteDetail), (MessageParser)(object)AdsRouletteDetail.Parser, new string[4] { "BaseInfo", "BaseData", "SlotList", "TargetList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
