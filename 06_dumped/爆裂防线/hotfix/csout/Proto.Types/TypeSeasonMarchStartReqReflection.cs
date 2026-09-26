using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeSeasonMarchStartReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeSeasonMarchStartReqReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch50eXBlLlNlYXNvbk1hcmNoU3RhcnRSZXEucHJvdG8SBXR5cGVzIl8KE1Nl" + "YXNvbk1hcmNoU3RhcnRSZXESDgoGVGVhbUlkGAEgASgFEg4KBkFjdGlvbhgC" + "IAEoBRILCgNUb1gYAyABKAISCwoDVG9ZGAQgASgCEg4KBlJlc0lkeBgFIAEo" + "BUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(SeasonMarchStartReq), (MessageParser)(object)SeasonMarchStartReq.Parser, new string[5] { "TeamId", "Action", "ToX", "ToY", "ResIdx" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
