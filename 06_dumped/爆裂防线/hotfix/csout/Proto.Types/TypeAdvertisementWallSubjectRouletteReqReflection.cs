using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeAdvertisementWallSubjectRouletteReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAdvertisementWallSubjectRouletteReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ci50eXBlLkFkdmVydGlzZW1lbnRXYWxsU3ViamVjdFJvdWxldHRlUmVxLnBy" + "b3RvEgV0eXBlcyJCCiNBZHZlcnRpc2VtZW50V2FsbFN1YmplY3RSb3VsZXR0" + "ZVJlcRINCgVBZHZJZBgBIAEoBRIMCgRVVUlEGAIgASgJQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AdvertisementWallSubjectRouletteReq), (MessageParser)(object)AdvertisementWallSubjectRouletteReq.Parser, new string[2] { "AdvId", "UUID" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
