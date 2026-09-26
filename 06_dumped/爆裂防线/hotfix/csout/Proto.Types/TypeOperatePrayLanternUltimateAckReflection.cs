using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperatePrayLanternUltimateAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperatePrayLanternUltimateAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cih0eXBlLk9wZXJhdGVQcmF5TGFudGVyblVsdGltYXRlQWNrLnByb3RvEgV0" + "eXBlcxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdl" + "LnByb3RvGhx0eXBlLlByYXlMYW50ZXJuRGV0YWlsLnByb3RvIqABCh1PcGVy" + "YXRlUHJheUxhbnRlcm5VbHRpbWF0ZUFjaxIxCg9QcmF5TGFudGVybkxpc3QY" + "ASADKAsyGC50eXBlcy5QcmF5TGFudGVybkRldGFpbBIlCglBd2FyZExpc3QY" + "AiADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgL" + "MhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypePrayLanternDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperatePrayLanternUltimateAck), (MessageParser)(object)OperatePrayLanternUltimateAck.Parser, new string[3] { "PrayLanternList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
