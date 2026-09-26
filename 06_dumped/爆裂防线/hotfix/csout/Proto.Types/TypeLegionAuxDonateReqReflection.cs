using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeLegionAuxDonateReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionAuxDonateReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkxlZ2lvbkF1eERvbmF0ZVJlcS5wcm90bxIFdHlwZXMaFWVudW0u" + "RU1vbmV5VHlwZS5wcm90byI6ChJMZWdpb25BdXhEb25hdGVSZXESJAoEVHlw" + "ZRgBIAEoDjIWLkVNb25leVR5cGUuRU1vbmV5VHlwZUIuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEMoneyTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionAuxDonateReq), (MessageParser)(object)LegionAuxDonateReq.Parser, new string[1] { "Type" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
