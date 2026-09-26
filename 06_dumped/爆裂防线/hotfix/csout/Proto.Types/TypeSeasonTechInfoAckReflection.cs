using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonTechInfoAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonTechInfoAckReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chx0eXBlLlNlYXNvblRlY2hJbmZvQWNrLnByb3RvEgV0eXBlcxoZdHlwZS5T" + "ZWFzb25UZWNoTm9kZS5wcm90byJPChFTZWFzb25UZWNoSW5mb0FjaxIRCglU" + "ZWNoUG9pbnQYASABKAUSJwoITm9kZUxpc3QYAiADKAsyFS50eXBlcy5TZWFz" + "b25UZWNoTm9kZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2Ft" + "ZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSeasonTechNodeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonTechInfoAck), (MessageParser)(object)SeasonTechInfoAck.Parser, new string[2] { "TechPoint", "NodeList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
