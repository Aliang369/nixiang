using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdvertisementWallSubjectRouletteAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdvertisementWallSubjectRouletteAckReflection()
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ci50eXBlLkFkdmVydGlzZW1lbnRXYWxsU3ViamVjdFJvdWxldHRlQWNrLnBy" + "b3RvEgV0eXBlcxocdHlwZS5BZHZlcnRpc2VtZW50SW5mby5wcm90bxoWdHlw" + "ZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGhx0" + "eXBlLldhbGxTdWJqZWN0RGV0YWlsLnByb3RvIuwBCiNBZHZlcnRpc2VtZW50" + "V2FsbFN1YmplY3RSb3VsZXR0ZUFjaxIzChFBZHZlcnRpc2VtZW50SW5mbxgB" + "IAEoCzIYLnR5cGVzLkFkdmVydGlzZW1lbnRJbmZvEjEKD1dhbGxTdWJqZWN0" + "TGlzdBgCIAMoCzIYLnR5cGVzLldhbGxTdWJqZWN0RGV0YWlsEg8KB0hpdExp" + "c3QYAyADKAUSJQoJQXdhcmRMaXN0GAQgAygLMhIudHlwZXMuQXdhcmREZXRh" + "aWwSJQoKRGF0YUNoYW5nZRgFIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJ" + "Li87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNi" + "BnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAdvertisementInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeWallSubjectDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdvertisementWallSubjectRouletteAck), (MessageParser)(object)AdvertisementWallSubjectRouletteAck.Parser, new string[5] { "AdvertisementInfo", "WallSubjectList", "HitList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
