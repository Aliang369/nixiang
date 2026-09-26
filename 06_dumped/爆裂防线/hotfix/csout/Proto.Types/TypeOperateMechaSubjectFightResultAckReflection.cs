using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateMechaSubjectFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateMechaSubjectFightResultAckReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cix0eXBlLk9wZXJhdGVNZWNoYVN1YmplY3RGaWdodFJlc3VsdEFjay5wcm90" + "bxIFdHlwZXMaFnR5cGUuQXdhcmREZXRhaWwucHJvdG8aG3R5cGUuQmF0dGxl" + "UmVzdWx0SW5mby5wcm90bxoVdHlwZS5EYXRhQ2hhbmdlLnByb3RvGh10eXBl" + "Lk1lY2hhU3ViamVjdERldGFpbC5wcm90byLVAQohT3BlcmF0ZU1lY2hhU3Vi" + "amVjdEZpZ2h0UmVzdWx0QWNrEjMKEE1lY2hhU3ViamVjdExpc3QYASADKAsy" + "GS50eXBlcy5NZWNoYVN1YmplY3REZXRhaWwSLQoMQmF0dGxlU2V0dGxlGAIg" + "ASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0SW5mbxIlCglBd2FyZExpc3QYAyAD" + "KAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRhQ2hhbmdlGAQgASgLMhEu" + "dHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oC" + "ElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeMechaSubjectDetailReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateMechaSubjectFightResultAck), (MessageParser)(object)OperateMechaSubjectFightResultAck.Parser, new string[4] { "MechaSubjectList", "BattleSettle", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
