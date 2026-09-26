using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionWarRoleStateInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarRoleStateInfoReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLkxlZ2lvbldhclJvbGVTdGF0ZUluZm8ucHJvdG8SBXR5cGVzGhpl" + "bnVtLkVMZWdpb25XYXJGb3JjZS5wcm90byKJAQoWTGVnaW9uV2FyUm9sZVN0" + "YXRlSW5mbxIxCgdGb3JjZUlkGAEgASgOMiAuRUxlZ2lvbldhckZvcmNlLkVM" + "ZWdpb25XYXJGb3JjZRITCgtBY3Rpb25MaW1pdBgCIAEoBRIUCgxBY3Rpb25Q" + "b2ludHMYAyABKAUSEQoJUHJvdGVjdFRtGAQgASgDQi5aCS4vO3Byb3RvY6oC" + "C1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumELegionWarForceReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarRoleStateInfo), (MessageParser)(object)LegionWarRoleStateInfo.Parser, new string[4] { "ForceId", "ActionLimit", "ActionPoints", "ProtectTm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
