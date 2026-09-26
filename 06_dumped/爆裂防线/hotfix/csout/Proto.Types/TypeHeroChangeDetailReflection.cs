using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeHeroChangeDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeHeroChangeDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkhlcm9DaGFuZ2VEZXRhaWwucHJvdG8SBXR5cGVzGhV0eXBlLkhl" + "cm9EZXRhaWwucHJvdG8iQgoQSGVyb0NoYW5nZURldGFpbBIhCgZVcGRhdGUY" + "ASADKAsyES50eXBlcy5IZXJvRGV0YWlsEgsKA0RlbBgCIAMoCUIuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeHeroDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(HeroChangeDetail), (MessageParser)(object)HeroChangeDetail.Parser, new string[2] { "Update", "Del" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
