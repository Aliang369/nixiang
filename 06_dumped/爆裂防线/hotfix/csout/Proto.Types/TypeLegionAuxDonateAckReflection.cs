using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeLegionAuxDonateAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeLegionAuxDonateAckReflection()
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkxlZ2lvbkF1eERvbmF0ZUFjay5wcm90bxIFdHlwZXMaFnR5cGUu" + "QXdhcmREZXRhaWwucHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90bxoVdHlw" + "ZS5MZWdpb25JbmZvLnByb3RvGhl0eXBlLkxlZ2lvblNlbGZJbmZvLnByb3Rv" + "IrgBChJMZWdpb25BdXhEb25hdGVBY2sSLQoOTGVnaW9uU2VsZkluZm8YASAB" + "KAsyFS50eXBlcy5MZWdpb25TZWxmSW5mbxIlCgpMZWdpb25JbmZvGAIgASgL" + "MhEudHlwZXMuTGVnaW9uSW5mbxIlCglBd2FyZExpc3QYAyADKAsyEi50eXBl" + "cy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAQgASgLMhEudHlwZXMuRGF0" + "YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeLegionInfoReflection.Descriptor,
			TypeLegionSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(LegionAuxDonateAck), (MessageParser)(object)LegionAuxDonateAck.Parser, new string[4] { "LegionSelfInfo", "LegionInfo", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
