using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLimitTimeRouletteDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLimitTimeRouletteDetailReflection()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLkxpbWl0VGltZVJvdWxldHRlRGV0YWlsLnByb3RvEgV0eXBlcxok" + "dHlwZS5MaW1pdFRpbWVSb3VsZXR0ZUJhc2VEYXRhLnByb3RvGiZ0eXBlLkxp" + "bWl0VGltZVJvdWxldHRlUmFuZERldGFpbC5wcm90bxoadHlwZS5PcGVyYXRl" + "QmFzZUluZm8ucHJvdG8aHnR5cGUuT3BlcmF0ZVRhcmdldERldGFpbC5wcm90" + "byLdAQoXTGltaXRUaW1lUm91bGV0dGVEZXRhaWwSKAoIQmFzZUluZm8YASAB" + "KAsyFi50eXBlcy5PcGVyYXRlQmFzZUluZm8SMgoIQmFzZURhdGEYAiABKAsy" + "IC50eXBlcy5MaW1pdFRpbWVSb3VsZXR0ZUJhc2VEYXRhEjQKCFJhbmRMaXN0" + "GAMgAygLMiIudHlwZXMuTGltaXRUaW1lUm91bGV0dGVSYW5kRGV0YWlsEi4K" + "ClRhcmdldExpc3QYBCADKAsyGi50eXBlcy5PcGVyYXRlVGFyZ2V0RGV0YWls" + "Qi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5" + "cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeLimitTimeRouletteBaseDataReflection.Descriptor,
			TypeLimitTimeRouletteRandDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateTargetDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LimitTimeRouletteDetail), (MessageParser)(object)LimitTimeRouletteDetail.Parser, new string[4] { "BaseInfo", "BaseData", "RandList", "TargetList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
