using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdvertisementLimitTimeTreasureReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdvertisementLimitTimeTreasureReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cix0eXBlLkFkdmVydGlzZW1lbnRMaW1pdFRpbWVUcmVhc3VyZVJlcS5wcm90" + "bxIFdHlwZXMiQAohQWR2ZXJ0aXNlbWVudExpbWl0VGltZVRyZWFzdXJlUmVx" + "Eg0KBUFkdklkGAEgASgFEgwKBFVVSUQYAiABKAlCLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdvertisementLimitTimeTreasureReq), (MessageParser)(object)AdvertisementLimitTimeTreasureReq.Parser, new string[2] { "AdvId", "UUID" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
