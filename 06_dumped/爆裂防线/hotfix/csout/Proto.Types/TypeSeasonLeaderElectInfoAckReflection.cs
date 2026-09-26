using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonLeaderElectInfoAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonLeaderElectInfoAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiN0eXBlLlNlYXNvbkxlYWRlckVsZWN0SW5mb0Fjay5wcm90bxIFdHlwZXMa" + "GnR5cGUuU2Vhc29uRWxlY3RJbmZvLnByb3RvIkAKGFNlYXNvbkxlYWRlckVs" + "ZWN0SW5mb0FjaxIkCgRJbmZvGAEgASgLMhYudHlwZXMuU2Vhc29uRWxlY3RJ" + "bmZvQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3Rv" + "XFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSeasonElectInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonLeaderElectInfoAck), (MessageParser)(object)SeasonLeaderElectInfoAck.Parser, new string[1] { "Info" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
