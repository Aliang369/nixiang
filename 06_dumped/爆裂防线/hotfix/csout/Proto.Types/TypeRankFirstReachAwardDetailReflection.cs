using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeRankFirstReachAwardDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRankFirstReachAwardDetailReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLlJhbmtGaXJzdFJlYWNoQXdhcmREZXRhaWwucHJvdG8SBXR5cGVz" + "GhRlbnVtLkVSYW5rVHlwZS5wcm90byJVChlSYW5rRmlyc3RSZWFjaEF3YXJk" + "RGV0YWlsEiYKCFJhbmtUeXBlGAEgASgOMhQuRVJhbmtUeXBlLkVSYW5rVHlw" + "ZRIQCghSZWNvcmRJZBgCIAEoBUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { EnumERankTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RankFirstReachAwardDetail), (MessageParser)(object)RankFirstReachAwardDetail.Parser, new string[2] { "RankType", "RecordId" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
