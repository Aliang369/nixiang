using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonTeamPosAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonTeamPosAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLlNlYXNvblRlYW1Qb3NBY2sucHJvdG8SBXR5cGVzGhx0eXBlLlNl" + "YXNvblRlYW1Qb3NJbmZvLnByb3RvIkEKEFNlYXNvblRlYW1Qb3NBY2sSLQoL" + "VGVhbVBvc0xpc3QYASADKAsyGC50eXBlcy5TZWFzb25UZWFtUG9zSW5mb0Iu" + "WgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBl" + "c2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeSeasonTeamPosInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonTeamPosAck), (MessageParser)(object)SeasonTeamPosAck.Parser, new string[1] { "TeamPosList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
