using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeJourneyChapterTwoDiamondCostAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeJourneyChapterTwoDiamondCostAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cip0eXBlLkpvdXJuZXlDaGFwdGVyVHdvRGlhbW9uZENvc3RBY2sucHJvdG8S" + "BXR5cGVzGhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvGhV0eXBlLkRhdGFDaGFu" + "Z2UucHJvdG8aFnR5cGUuSm91cm5leUluZm8ucHJvdG8imAEKH0pvdXJuZXlD" + "aGFwdGVyVHdvRGlhbW9uZENvc3RBY2sSJwoLSm91cm5leUluZm8YASABKAsy" + "Ei50eXBlcy5Kb3VybmV5SW5mbxIlCglBd2FyZExpc3QYAiADKAsyEi50eXBl" + "cy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlwZXMuRGF0" + "YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeJourneyInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(JourneyChapterTwoDiamondCostAck), (MessageParser)(object)JourneyChapterTwoDiamondCostAck.Parser, new string[3] { "JourneyInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
