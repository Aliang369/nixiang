using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeFacilityRollingEndAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeFacilityRollingEndAckReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkZhY2lsaXR5Um9sbGluZ0VuZEFjay5wcm90bxIFdHlwZXMaFnR5" + "cGUuQXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90bxoa" + "dHlwZS5GYWNpbGl0eU1hcEluZm8ucHJvdG8aIHR5cGUuRmFjaWxpdHlSb2xs" + "aW5nRGV0YWlsLnByb3RvIsUBChVGYWNpbGl0eVJvbGxpbmdFbmRBY2sSLwoP" + "RmFjaWxpdHlNYXBJbmZvGAEgASgLMhYudHlwZXMuRmFjaWxpdHlNYXBJbmZv" + "Ei0KB1JvbGxpbmcYAiABKAsyHC50eXBlcy5GYWNpbGl0eVJvbGxpbmdEZXRh" + "aWwSJQoJQXdhcmRMaXN0GAMgAygLMhIudHlwZXMuQXdhcmREZXRhaWwSJQoK" + "RGF0YUNoYW5nZRgEIAEoCzIRLnR5cGVzLkRhdGFDaGFuZ2VCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeFacilityMapInfoReflection.Descriptor,
			TypeFacilityRollingDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(FacilityRollingEndAck), (MessageParser)(object)FacilityRollingEndAck.Parser, new string[4] { "FacilityMapInfo", "Rolling", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
