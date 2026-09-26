using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionWarPlayerRankListAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionWarPlayerRankListAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLkxlZ2lvbldhclBsYXllclJhbmtMaXN0QWNrLnByb3RvEgV0eXBl" + "cxokdHlwZS5MZWdpb25XYXJQbGF5ZXJSYW5rRGV0YWlsLnByb3RvIlAKGkxl" + "Z2lvbldhclBsYXllclJhbmtMaXN0QWNrEjIKCFJhbmtMaXN0GAEgAygLMiAu" + "dHlwZXMuTGVnaW9uV2FyUGxheWVyUmFua0RldGFpbEIuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeLegionWarPlayerRankDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionWarPlayerRankListAck), (MessageParser)(object)LegionWarPlayerRankListAck.Parser, new string[1] { "RankList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
