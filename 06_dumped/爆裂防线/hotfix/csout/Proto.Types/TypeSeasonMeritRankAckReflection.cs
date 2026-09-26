using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonMeritRankAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonMeritRankAckReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlNlYXNvbk1lcml0UmFua0Fjay5wcm90bxIFdHlwZXMaGXR5cGUu" + "U2Vhc29uUmFua0l0ZW0ucHJvdG8iOQoSU2Vhc29uTWVyaXRSYW5rQWNrEiMK" + "BExpc3QYASADKAsyFS50eXBlcy5TZWFzb25SYW5rSXRlbUIuWgkuLztwcm90" + "b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSeasonRankItemReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonMeritRankAck), (MessageParser)(object)SeasonMeritRankAck.Parser, new string[1] { "List" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
