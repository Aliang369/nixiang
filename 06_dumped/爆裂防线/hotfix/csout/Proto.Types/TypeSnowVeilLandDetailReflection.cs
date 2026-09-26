using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSnowVeilLandDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSnowVeilLandDetailReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlNub3dWZWlsTGFuZERldGFpbC5wcm90bxIFdHlwZXMaF2VudW0u" + "RUNvbW1vblN0YXRlLnByb3RvGhxlbnVtLkVTbm93VmVpbExhbmRUeXBlLnBy" + "b3RvGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvIrsBChJTbm93VmVpbExhbmRE" + "ZXRhaWwSDQoFSW5kZXgYASABKAUSNgoITGFuZFR5cGUYAiABKA4yJC5FU25v" + "d1ZlaWxMYW5kVHlwZS5FU25vd1ZlaWxMYW5kVHlwZRItCglMYW5kU3RhdGUY" + "AyABKA4yGi5FQ29tbW9uU3RhdGUuRUNvbW1vblN0YXRlEg0KBVNjb3JlGAQg" + "ASgFEiAKBEl0ZW0YBSABKAsyEi50eXBlcy5Bd2FyZERldGFpbEIuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumECommonStateReflection.Descriptor,
			EnumESnowVeilLandTypeReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SnowVeilLandDetail), (MessageParser)(object)SnowVeilLandDetail.Parser, new string[5] { "Index", "LandType", "LandState", "Score", "Item" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
