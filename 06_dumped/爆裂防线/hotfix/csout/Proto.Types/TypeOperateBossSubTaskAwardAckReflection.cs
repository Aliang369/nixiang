using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateBossSubTaskAwardAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateBossSubTaskAwardAckReflection()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiV0eXBlLk9wZXJhdGVCb3NzU3ViVGFza0F3YXJkQWNrLnByb3RvEgV0eXBl" + "cxoWdHlwZS5Bd2FyZERldGFpbC5wcm90bxocdHlwZS5Cb3NzU3ViamVjdERl" + "dGFpbC5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvIp0BChpPcGVyYXRl" + "Qm9zc1N1YlRhc2tBd2FyZEFjaxIxCg9Cb3NzU3ViamVjdExpc3QYASADKAsy" + "GC50eXBlcy5Cb3NzU3ViamVjdERldGFpbBIlCglBd2FyZExpc3QYAiADKAsy" + "Ei50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAMgASgLMhEudHlw" + "ZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[3]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBossSubjectDetailReflection.Descriptor,
			TypeDataChangeReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateBossSubTaskAwardAck), (MessageParser)(object)OperateBossSubTaskAwardAck.Parser, new string[3] { "BossSubjectList", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
