using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLimitVipHeroDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLimitVipHeroDetailReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkxpbWl0VmlwSGVyb0RldGFpbC5wcm90bxIFdHlwZXMaH3R5cGUu" + "TGltaXRWaXBIZXJvQmFzZURhdGEucHJvdG8aIXR5cGUuTGltaXRWaXBIZXJv" + "UmFuZERldGFpbC5wcm90bxoadHlwZS5PcGVyYXRlQmFzZUluZm8ucHJvdG8a" + "HnR5cGUuT3BlcmF0ZVRhcmdldERldGFpbC5wcm90byLOAQoSTGltaXRWaXBI" + "ZXJvRGV0YWlsEigKCEJhc2VJbmZvGAEgASgLMhYudHlwZXMuT3BlcmF0ZUJh" + "c2VJbmZvEi0KCEJhc2VEYXRhGAIgASgLMhsudHlwZXMuTGltaXRWaXBIZXJv" + "QmFzZURhdGESLwoIUmFuZExpc3QYAyADKAsyHS50eXBlcy5MaW1pdFZpcEhl" + "cm9SYW5kRGV0YWlsEi4KClRhcmdldExpc3QYBCADKAsyGi50eXBlcy5PcGVy" + "YXRlVGFyZ2V0RGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygIS" + "WmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeLimitVipHeroBaseDataReflection.Descriptor,
			TypeLimitVipHeroRandDetailReflection.Descriptor,
			TypeOperateBaseInfoReflection.Descriptor,
			TypeOperateTargetDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LimitVipHeroDetail), (MessageParser)(object)LimitVipHeroDetail.Parser, new string[4] { "BaseInfo", "BaseData", "RandList", "TargetList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
