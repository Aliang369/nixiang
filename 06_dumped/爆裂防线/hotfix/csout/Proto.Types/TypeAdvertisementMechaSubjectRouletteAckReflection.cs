using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdvertisementMechaSubjectRouletteAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdvertisementMechaSubjectRouletteAckReflection()
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ci90eXBlLkFkdmVydGlzZW1lbnRNZWNoYVN1YmplY3RSb3VsZXR0ZUFjay5w" + "cm90bxIFdHlwZXMaHHR5cGUuQWR2ZXJ0aXNlbWVudEluZm8ucHJvdG8aFnR5" + "cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90bxod" + "dHlwZS5NZWNoYVN1YmplY3REZXRhaWwucHJvdG8i7wEKJEFkdmVydGlzZW1l" + "bnRNZWNoYVN1YmplY3RSb3VsZXR0ZUFjaxIzChFBZHZlcnRpc2VtZW50SW5m" + "bxgBIAEoCzIYLnR5cGVzLkFkdmVydGlzZW1lbnRJbmZvEjMKEE1lY2hhU3Vi" + "amVjdExpc3QYAiADKAsyGS50eXBlcy5NZWNoYVN1YmplY3REZXRhaWwSDwoH" + "SGl0TGlzdBgDIAMoBRIlCglBd2FyZExpc3QYBCADKAsyEi50eXBlcy5Bd2Fy" + "ZERldGFpbBIlCgpEYXRhQ2hhbmdlGAUgASgLMhEudHlwZXMuRGF0YUNoYW5n" + "ZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAdvertisementInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMechaSubjectDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdvertisementMechaSubjectRouletteAck), (MessageParser)(object)AdvertisementMechaSubjectRouletteAck.Parser, new string[5] { "AdvertisementInfo", "MechaSubjectList", "HitList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
