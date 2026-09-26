using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityShopBuyAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityShopBuyAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkZhY2lsaXR5U2hvcEJ1eUFjay5wcm90bxIFdHlwZXMaFnR5cGUu" + "QXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90bxoadHlw" + "ZS5GYWNpbGl0eU1hcEluZm8ucHJvdG8aG3R5cGUuRmFjaWxpdHlTaG9wSW5m" + "by5wcm90byLGAQoSRmFjaWxpdHlTaG9wQnV5QWNrEi8KD0ZhY2lsaXR5TWFw" + "SW5mbxgBIAEoCzIWLnR5cGVzLkZhY2lsaXR5TWFwSW5mbxIxChBGYWNpbGl0" + "eVNob3BJbmZvGAIgASgLMhcudHlwZXMuRmFjaWxpdHlTaG9wSW5mbxIlCglB" + "d2FyZExpc3QYAyADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hh" + "bmdlGAQgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeFacilityMapInfoReflection.Descriptor,
			TypeFacilityShopInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityShopBuyAck), (MessageParser)(object)FacilityShopBuyAck.Parser, new string[4] { "FacilityMapInfo", "FacilityShopInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
