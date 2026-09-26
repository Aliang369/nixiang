using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeDataChangeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeDataChangeReflection()
	{
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Expected O, but got Unknown
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChV0eXBlLkRhdGFDaGFuZ2UucHJvdG8SBXR5cGVzGhx0eXBlLkVxdWlwQ2hh" + "bmdlRGV0YWlsLnByb3RvGht0eXBlLkhlcm9DaGFuZ2VEZXRhaWwucHJvdG8a" + "G3R5cGUuUHJvcENoYW5nZURldGFpbC5wcm90bxofdHlwZS5SZXNvdXJjZUNo" + "YW5nZURldGFpbC5wcm90bxoXdHlwZS5Sb2xlQmFzZUluZm8ucHJvdG8aGXR5" + "cGUuUm9sZUNvbW1vbkluZm8ucHJvdG8aF3R5cGUuUm9sZVRlYW1JbmZvLnBy" + "b3RvGh10eXBlLlRhY3RpY0NoYW5nZURldGFpbC5wcm90byLpBAoKRGF0YUNo" + "YW5nZRIpCgxSb2xlQmFzZUluZm8YASABKAsyEy50eXBlcy5Sb2xlQmFzZUlu" + "Zm8SLQoOUm9sZUNvbW1vbkluZm8YAiABKAsyFS50eXBlcy5Sb2xlQ29tbW9u" + "SW5mbxIpCgxSb2xlVGVhbUluZm8YAyABKAsyEy50eXBlcy5Sb2xlVGVhbUlu" + "Zm8SKgoFTW9uZXkYBCABKAsyGy50eXBlcy5SZXNvdXJjZUNoYW5nZURldGFp" + "bBIpCgRJdGVtGAUgASgLMhsudHlwZXMuUmVzb3VyY2VDaGFuZ2VEZXRhaWwS" + "LQoISGVyb0ZyYWcYBiABKAsyGy50eXBlcy5SZXNvdXJjZUNoYW5nZURldGFp" + "bBIoCgNCb3gYByABKAsyGy50eXBlcy5SZXNvdXJjZUNoYW5nZURldGFpbBIo" + "CgNHZW0YCCABKAsyGy50eXBlcy5SZXNvdXJjZUNoYW5nZURldGFpbBIlCgRQ" + "cm9wGAkgASgLMhcudHlwZXMuUHJvcENoYW5nZURldGFpbBInCgVFcXVpcBgK" + "IAEoCzIYLnR5cGVzLkVxdWlwQ2hhbmdlRGV0YWlsEiUKBEhlcm8YCyABKAsy" + "Fy50eXBlcy5IZXJvQ2hhbmdlRGV0YWlsEikKBEdlbmUYDCABKAsyGy50eXBl" + "cy5SZXNvdXJjZUNoYW5nZURldGFpbBIvCgpUYWN0aWNGcmFnGA0gASgLMhsu" + "dHlwZXMuUmVzb3VyY2VDaGFuZ2VEZXRhaWwSKQoGVGFjdGljGA4gASgLMhku" + "dHlwZXMuVGFjdGljQ2hhbmdlRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[8]
		{
			TypeEquipChangeDetailReflection.Descriptor,
			TypeHeroChangeDetailReflection.Descriptor,
			TypePropChangeDetailReflection.Descriptor,
			TypeResourceChangeDetailReflection.Descriptor,
			TypeRoleBaseInfoReflection.Descriptor,
			TypeRoleCommonInfoReflection.Descriptor,
			TypeRoleTeamInfoReflection.Descriptor,
			TypeTacticChangeDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(DataChange), (MessageParser)(object)DataChange.Parser, new string[14]
			{
				"RoleBaseInfo", "RoleCommonInfo", "RoleTeamInfo", "Money", "Item", "HeroFrag", "Box", "Gem", "Prop", "Equip",
				"Hero", "Gene", "TacticFrag", "Tactic"
			}, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
