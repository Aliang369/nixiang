using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRoleTeamInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRoleTeamInfoReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chd0eXBlLlJvbGVUZWFtSW5mby5wcm90bxIFdHlwZXMaH3R5cGUuQmF0dGxl" + "VGVhbUF0dHJEZXRhaWwucHJvdG8iTgoMUm9sZVRlYW1JbmZvEi0KCEF0dHJM" + "aXN0GAEgAygLMhsudHlwZXMuQmF0dGxlVGVhbUF0dHJEZXRhaWwSDwoHTW9k" + "TGlzdBgCIAMoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleTeamAttrDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RoleTeamInfo), (MessageParser)(object)RoleTeamInfo.Parser, new string[2] { "AttrList", "ModList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
