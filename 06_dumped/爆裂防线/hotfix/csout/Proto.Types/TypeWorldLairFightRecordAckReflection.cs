using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldLairFightRecordAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldLairFightRecordAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiJ0eXBlLldvcmxkTGFpckZpZ2h0UmVjb3JkQWNrLnByb3RvEgV0eXBlcxol" + "dHlwZS5Xb3JsZExhaXJGaWdodFJlY29yZERldGFpbC5wcm90byJQChdXb3Js" + "ZExhaXJGaWdodFJlY29yZEFjaxI1CgpSZWNvcmRMaXN0GAEgAygLMiEudHlw" + "ZXMuV29ybGRMYWlyRmlnaHRSZWNvcmREZXRhaWxCLloJLi87cHJvdG9jqgIL" + "UHJvdG8uVHlwZXPKAhJaYmdhbWVcUHJvdG9cVHlwZXNiBnByb3RvMw=="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeWorldLairFightRecordDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldLairFightRecordAck), (MessageParser)(object)WorldLairFightRecordAck.Parser, new string[1] { "RecordList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
