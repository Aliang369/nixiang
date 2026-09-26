using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossPeakDataAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossPeakDataAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cht0eXBlLkNyb3NzUGVha0RhdGFBY2sucHJvdG8SBXR5cGVzGh50eXBlLkNy" + "b3NzUGVha1JhbmtEZXRhaWwucHJvdG8aHHR5cGUuQ3Jvc3NQZWFrUmFua0lu" + "Zm8ucHJvdG8aHHR5cGUuQ3Jvc3NQZWFrU2VsZkluZm8ucHJvdG8iqgEKEENy" + "b3NzUGVha0RhdGFBY2sSMwoRQ3Jvc3NQZWFrU2VsZkluZm8YASABKAsyGC50" + "eXBlcy5Dcm9zc1BlYWtTZWxmSW5mbxIzChFDcm9zc1BlYWtSYW5rSW5mbxgC" + "IAEoCzIYLnR5cGVzLkNyb3NzUGVha1JhbmtJbmZvEiwKCFJhbmtMaXN0GAMg" + "AygLMhoudHlwZXMuQ3Jvc3NQZWFrUmFua0RldGFpbEIuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeCrossPeakRankDetailReflection.Descriptor,
			TypeCrossPeakRankInfoReflection.Descriptor,
			TypeCrossPeakSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossPeakDataAck), (MessageParser)(object)CrossPeakDataAck.Parser, new string[3] { "CrossPeakSelfInfo", "CrossPeakRankInfo", "RankList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
