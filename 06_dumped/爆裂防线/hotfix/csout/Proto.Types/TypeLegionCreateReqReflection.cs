using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionCreateReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionCreateReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLkxlZ2lvbkNyZWF0ZVJlcS5wcm90bxIFdHlwZXMaFWVudW0uRUJv" + "b2xTdGF0ZS5wcm90byJtCg9MZWdpb25DcmVhdGVSZXESEgoKTGVnaW9uTmFt" + "ZRgBIAEoCRIMCgRJY29uGAIgASgFEg8KB0pvaW5MdmwYAyABKAUSJwoHQ29u" + "ZmlybRgEIAEoDjIWLkVCb29sU3RhdGUuRUJvb2xTdGF0ZUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEBoolStateReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionCreateReq), (MessageParser)(object)LegionCreateReq.Parser, new string[4] { "LegionName", "Icon", "JoinLvl", "Confirm" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
