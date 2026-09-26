using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdvertisementMechaSubjectRouletteReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdvertisementMechaSubjectRouletteReqReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ci90eXBlLkFkdmVydGlzZW1lbnRNZWNoYVN1YmplY3RSb3VsZXR0ZVJlcS5w" + "cm90bxIFdHlwZXMiQwokQWR2ZXJ0aXNlbWVudE1lY2hhU3ViamVjdFJvdWxl" + "dHRlUmVxEg0KBUFkdklkGAEgASgFEgwKBFVVSUQYAiABKAlCLloJLi87cHJv" + "dG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3Rv" + "Mw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdvertisementMechaSubjectRouletteReq), (MessageParser)(object)AdvertisementMechaSubjectRouletteReq.Parser, new string[2] { "AdvId", "UUID" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
