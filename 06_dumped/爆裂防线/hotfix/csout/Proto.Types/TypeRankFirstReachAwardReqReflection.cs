using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeRankFirstReachAwardReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRankFirstReachAwardReqReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlJhbmtGaXJzdFJlYWNoQXdhcmRSZXEucHJvdG8SBXR5cGVzGhRl" + "bnVtLkVSYW5rVHlwZS5wcm90byJAChZSYW5rRmlyc3RSZWFjaEF3YXJkUmVx" + "EiYKCFJhbmtUeXBlGAEgASgOMhQuRVJhbmtUeXBlLkVSYW5rVHlwZUIuWgku" + "Lztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IG" + "cHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumERankTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RankFirstReachAwardReq), (MessageParser)(object)RankFirstReachAwardReq.Parser, new string[1] { "RankType" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
