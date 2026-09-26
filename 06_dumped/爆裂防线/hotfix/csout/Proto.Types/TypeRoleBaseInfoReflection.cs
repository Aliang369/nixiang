using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRoleBaseInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleBaseInfoReflection()
	{
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLlJvbGVCYXNlSW5mby5wcm90bxIFdHlwZXMihAIKDFJvbGVCYXNl" + "SW5mbxIOCgZSb2xlSWQYASABKAMSEAoIUm9sZU5hbWUYAiABKAkSEAoIU2Vy" + "dmVySWQYAyABKAUSDQoFTGV2ZWwYBCABKAUSEAoIVmlwTGV2ZWwYBSABKAUS" + "EAoIQXZhdGFySWQYBiABKAUSDgoGU3VpdElkGAcgASgFEg0KBUd1bklkGAgg" + "ASgFEhAKCFNsb2dhbklkGAkgASgFEg4KBkhlYWRJZBgKIAEoBRITCgtIZWFk" + "RnJhbWVJZBgLIAEoBRISCgpUZWFtQXR0YWNrGAwgASgFEg8KB01lY2hhSWQY" + "DSABKAUSEgoKRm9vdGhvbGRJZBgOIAEoBUIuWgkuLztwcm90b2OqAgtQcm90" + "by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleBaseInfo), (MessageParser)(object)RoleBaseInfo.Parser, new string[14]
			{
				"RoleId", "RoleName", "ServerId", "Level", "VipLevel", "AvatarId", "SuitId", "GunId", "SloganId", "HeadId",
				"HeadFrameId", "TeamAttack", "MechaId", "FootholdId"
			}, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
