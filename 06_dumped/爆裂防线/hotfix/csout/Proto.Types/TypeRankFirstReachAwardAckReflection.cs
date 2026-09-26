using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeRankFirstReachAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeRankFirstReachAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlJhbmtGaXJzdFJlYWNoQXdhcmRBY2sucHJvdG8SBXR5cGVzGhd0" + "eXBlLkF1eEF3YXJkSW5mby5wcm90bxoWdHlwZS5Bd2FyZERldGFpbC5wcm90" + "bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvIpEBChZSYW5rRmlyc3RSZWFjaEF3" + "YXJkQWNrEikKDEF1eEF3YXJkSW5mbxgBIAEoCzITLnR5cGVzLkF1eEF3YXJk" + "SW5mbxIlCglBd2FyZExpc3QYAiADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIl" + "CgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztw" + "cm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJv" + "dG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAuxAwardInfoReflection.Descriptor,
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(RankFirstReachAwardAck), (MessageParser)(object)RankFirstReachAwardAck.Parser, new string[3] { "AuxAwardInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
