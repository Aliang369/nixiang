using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeArenaRankListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeArenaRankListAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkFyZW5hUmFua0xpc3RBY2sucHJvdG8SBXR5cGVzGhN0eXBlLlJh" + "bmtEYXRhLnByb3RvGhV0eXBlLlJhbmtEZXRhaWwucHJvdG8iWgoQQXJlbmFS" + "YW5rTGlzdEFjaxIjCghSYW5rTGlzdBgBIAMoCzIRLnR5cGVzLlJhbmtEZXRh" + "aWwSIQoIU2VsZlJhbmsYAiABKAsyDy50eXBlcy5SYW5rRGF0YUIuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeRankDataReflection.Descriptor,
			TypeRankDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ArenaRankListAck), (MessageParser)(object)ArenaRankListAck.Parser, new string[2] { "RankList", "SelfRank" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
