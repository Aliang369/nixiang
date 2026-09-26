using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeSeasonCityWarInfoAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonCityWarInfoAckReflection()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch90eXBlLlNlYXNvbkNpdHlXYXJJbmZvQWNrLnByb3RvEgV0eXBlcxoeZW51" + "bS5FU2Vhc29uQ2l0eVdhclN0YXRlLnByb3RvGhx0eXBlLlNlYXNvbkRlY2xh" + "cmVJbmZvLnByb3RvGh10eXBlLlNlYXNvbk9ubGluZVJld2FyZC5wcm90byK2" + "AQoUU2Vhc29uQ2l0eVdhckluZm9BY2sSNwoFU3RhdGUYASABKA4yKC5FU2Vh" + "c29uQ2l0eVdhclN0YXRlLkVTZWFzb25DaXR5V2FyU3RhdGUSDQoFRW5kVG0Y" + "AiABKAMSKgoIRGVjbGFyZXMYAyADKAsyGC50eXBlcy5TZWFzb25EZWNsYXJl" + "SW5mbxIqCgdSZXdhcmRzGAQgAygLMhkudHlwZXMuU2Vhc29uT25saW5lUmV3" + "YXJkQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			EnumESeasonCityWarStateReflection.Descriptor,
			TypeSeasonDeclareInfoReflection.Descriptor,
			TypeSeasonOnlineRewardReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonCityWarInfoAck), (MessageParser)(object)SeasonCityWarInfoAck.Parser, new string[4] { "State", "EndTm", "Declares", "Rewards" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
