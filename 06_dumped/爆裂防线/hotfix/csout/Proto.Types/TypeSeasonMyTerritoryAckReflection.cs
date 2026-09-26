using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonMyTerritoryAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonMyTerritoryAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLlNlYXNvbk15VGVycml0b3J5QWNrLnByb3RvEgV0eXBlcxocdHlw" + "ZS5TZWFzb25NeVRlcnJpdG9yeS5wcm90byJHChRTZWFzb25NeVRlcnJpdG9y" + "eUFjaxIvCg1UZXJyaXRvcnlMaXN0GAEgAygLMhgudHlwZXMuU2Vhc29uTXlU" + "ZXJyaXRvcnlCLloJLi87cHJvdG9jqgILUHJvdG8uVHlwZXPKAhJaYmdhbWVc" + "UHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSeasonMyTerritoryReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonMyTerritoryAck), (MessageParser)(object)SeasonMyTerritoryAck.Parser, new string[1] { "TerritoryList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
