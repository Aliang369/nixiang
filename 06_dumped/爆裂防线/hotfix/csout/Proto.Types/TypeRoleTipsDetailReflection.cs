using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRoleTipsDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleTipsDetailReflection()
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chl0eXBlLlJvbGVUaXBzRGV0YWlsLnByb3RvEgV0eXBlcxoZdHlwZS5Gb290" + "aG9sZERldGFpbC5wcm90bxoWdHlwZS5NZWNoYURldGFpbC5wcm90bxoadHlw" + "ZS5QbGF5ZXJHdW5EZXRhaWwucHJvdG8aG3R5cGUuUGxheWVyU3VpdERldGFp" + "bC5wcm90bxoVdHlwZS5XYWxsRGV0YWlsLnByb3RvIt0BCg5Sb2xlVGlwc0Rl" + "dGFpbBInCgdHdW5MaXN0GAEgAygLMhYudHlwZXMuUGxheWVyR3VuRGV0YWls" + "EikKCFN1aXRMaXN0GAIgAygLMhcudHlwZXMuUGxheWVyU3VpdERldGFpbBIj" + "CghXYWxsTGlzdBgDIAMoCzIRLnR5cGVzLldhbGxEZXRhaWwSJQoJTWVjaGFM" + "aXN0GAQgAygLMhIudHlwZXMuTWVjaGFEZXRhaWwSKwoMRm9vdGhvbGRMaXN0" + "GAUgAygLMhUudHlwZXMuRm9vdGhvbGREZXRhaWxCLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeFootholdDetailReflection.Descriptor,
			TypeMechaDetailReflection.Descriptor,
			TypePlayerGunDetailReflection.Descriptor,
			TypePlayerSuitDetailReflection.Descriptor,
			TypeWallDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleTipsDetail), (MessageParser)(object)RoleTipsDetail.Parser, new string[5] { "GunList", "SuitList", "WallList", "MechaList", "FootholdList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
