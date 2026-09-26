using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarSelfTeamInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarSelfTeamInfoReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiB0eXBlLkxlZ2lvbldhclNlbGZUZWFtSW5mby5wcm90bxIFdHlwZXMaFnR5" + "cGUuU2tpbGxEZXRhaWwucHJvdG8iZgoVTGVnaW9uV2FyU2VsZlRlYW1JbmZv" + "EiUKCVNraWxsTGlzdBgBIAMoCzISLnR5cGVzLlNraWxsRGV0YWlsEg8KB01v" + "ZExpc3QYAiADKAUSFQoNTW9kU2VsZWN0TGlzdBgDIAMoBUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSkillDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarSelfTeamInfo), (MessageParser)(object)LegionWarSelfTeamInfo.Parser, new string[3] { "SkillList", "ModList", "ModSelectList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
