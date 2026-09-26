using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWarRoomHeroSetAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWarRoomHeroSetAckReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLldhclJvb21IZXJvU2V0QWNrLnByb3RvEgV0eXBlcxoWdHlwZS5X" + "YXJSb29tSW5mby5wcm90byI8ChFXYXJSb29tSGVyb1NldEFjaxInCgtXYXJS" + "b29tSW5mbxgBIAEoCzISLnR5cGVzLldhclJvb21JbmZvQi5aCS4vO3Byb3Rv" + "Y6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeWarRoomInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WarRoomHeroSetAck), (MessageParser)(object)WarRoomHeroSetAck.Parser, new string[1] { "WarRoomInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
