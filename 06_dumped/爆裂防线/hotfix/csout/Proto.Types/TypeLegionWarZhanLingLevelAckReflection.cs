using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarZhanLingLevelAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarZhanLingLevelAckReflection()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLkxlZ2lvbldhclpoYW5MaW5nTGV2ZWxBY2sucHJvdG8SBXR5cGVz" + "Ghx0eXBlLkxlZ2lvbldhckJhc2VJbmZvLnByb3RvGiB0eXBlLkxlZ2lvbldh" + "clNlbGZUZWFtSW5mby5wcm90byKNAQoZTGVnaW9uV2FyWmhhbkxpbmdMZXZl" + "bEFjaxIzChFMZWdpb25XYXJCYXNlSW5mbxgBIAEoCzIYLnR5cGVzLkxlZ2lv" + "bldhckJhc2VJbmZvEjsKFUxlZ2lvbldhclNlbGZUZWFtSW5mbxgCIAEoCzIc" + "LnR5cGVzLkxlZ2lvbldhclNlbGZUZWFtSW5mb0IuWgkuLztwcm90b2OqAgtQ" + "cm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeLegionWarBaseInfoReflection.Descriptor,
			TypeLegionWarSelfTeamInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarZhanLingLevelAck), (MessageParser)(object)LegionWarZhanLingLevelAck.Parser, new string[2] { "LegionWarBaseInfo", "LegionWarSelfTeamInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
