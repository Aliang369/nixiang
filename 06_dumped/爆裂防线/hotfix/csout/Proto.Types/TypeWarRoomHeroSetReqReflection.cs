using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeWarRoomHeroSetReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWarRoomHeroSetReqReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLldhclJvb21IZXJvU2V0UmVxLnByb3RvEgV0eXBlcxoVZW51bS5F" + "U2tpbGxUeXBlLnByb3RvIk8KEVdhclJvb21IZXJvU2V0UmVxEikKCVNraWxs" + "VHlwZRgBIAEoDjIWLkVTa2lsbFR5cGUuRVNraWxsVHlwZRIPCgdIZXJvVGlk" + "GAIgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFBy" + "b3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumESkillTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WarRoomHeroSetReq), (MessageParser)(object)WarRoomHeroSetReq.Parser, new string[2] { "SkillType", "HeroTid" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
