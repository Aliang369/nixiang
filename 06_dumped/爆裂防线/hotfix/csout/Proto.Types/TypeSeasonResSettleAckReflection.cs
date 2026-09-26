using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonResSettleAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonResSettleAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLlNlYXNvblJlc1NldHRsZUFjay5wcm90bxIFdHlwZXMaHHR5cGUu" + "U2Vhc29uUHJvZHVjZUl0ZW0ucHJvdG8iZgoSU2Vhc29uUmVzU2V0dGxlQWNr" + "Eg8KB0dhc0dhaW4YASABKAUSEAoIVGVjaEdhaW4YAiABKAUSLQoLUHJvZHVj" + "ZUxpc3QYAyADKAsyGC50eXBlcy5TZWFzb25Qcm9kdWNlSXRlbUIuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSeasonProduceItemReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonResSettleAck), (MessageParser)(object)SeasonResSettleAck.Parser, new string[3] { "GasGain", "TechGain", "ProduceList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
