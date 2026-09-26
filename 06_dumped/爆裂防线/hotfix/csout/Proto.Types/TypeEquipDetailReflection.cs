using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeEquipDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeEquipDetailReflection()
	{
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLkVxdWlwRGV0YWlsLnByb3RvEgV0eXBlcxoVZW51bS5FQm9vbFN0" + "YXRlLnByb3RvGhtlbnVtLkVFcXVpcFJhcml0eVR5cGUucHJvdG8aE2VudW0u" + "RVF1YWxpdHkucHJvdG8aHXR5cGUuRXF1aXBTdWJBdHRyRGV0YWlsLnByb3Rv" + "IvEBCgtFcXVpcERldGFpbBIPCgdFcXVpcElkGAEgASgJEhAKCEVxdWlwVGlk" + "GAIgASgFEiMKB1F1YWxpdHkYAyABKA4yEi5FUXVhbGl0eS5FUXVhbGl0eRIy" + "CgZSYXJpdHkYBCABKA4yIi5FRXF1aXBSYXJpdHlUeXBlLkVFcXVpcFJhcml0" + "eVR5cGUSEAoIU3ViTGV2ZWwYBSABKAUSLgoLU3ViQXR0ckxpc3QYBiADKAsy" + "GS50eXBlcy5FcXVpcFN1YkF0dHJEZXRhaWwSJAoETG9jaxgHIAEoDjIWLkVC" + "b29sU3RhdGUuRUJvb2xTdGF0ZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			EnumEBoolStateReflection.Descriptor,
			EnumEEquipRarityTypeReflection.Descriptor,
			EnumEQualityReflection.Descriptor,
			TypeEquipSubAttrDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(EquipDetail), (MessageParser)(object)EquipDetail.Parser, new string[7] { "EquipId", "EquipTid", "Quality", "Rarity", "SubLevel", "SubAttrList", "Lock" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
