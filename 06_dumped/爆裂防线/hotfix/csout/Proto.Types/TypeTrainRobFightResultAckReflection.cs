using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeTrainRobFightResultAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeTrainRobFightResultAckReflection()
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiF0eXBlLlRyYWluUm9iRmlnaHRSZXN1bHRBY2sucHJvdG8SBXR5cGVzGhZ0" + "eXBlLkF3YXJkRGV0YWlsLnByb3RvGht0eXBlLkJhdHRsZVJlc3VsdEluZm8u" + "cHJvdG8aFXR5cGUuRGF0YUNoYW5nZS5wcm90bxoWdHlwZS5UcmFpbkRldGFp" + "bC5wcm90bxoUdHlwZS5UcmFpbkluZm8ucHJvdG8i4AEKFlRyYWluUm9iRmln" + "aHRSZXN1bHRBY2sSIwoJVHJhaW5JbmZvGAEgASgLMhAudHlwZXMuVHJhaW5J" + "bmZvEiQKCFJvYlRyYWluGAIgASgLMhIudHlwZXMuVHJhaW5EZXRhaWwSLQoM" + "QmF0dGxlU2V0dGxlGAMgASgLMhcudHlwZXMuQmF0dGxlUmVzdWx0SW5mbxIl" + "CglBd2FyZExpc3QYBCADKAsyEi50eXBlcy5Bd2FyZERldGFpbBIlCgpEYXRh" + "Q2hhbmdlGAUgASgLMhEudHlwZXMuRGF0YUNoYW5nZUIuWgkuLztwcm90b2Oq" + "AgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[5]
		{
			TypeAwardDetailReflection.Descriptor,
			TypeBattleResultInfoReflection.Descriptor,
			TypeDataChangeReflection.Descriptor,
			TypeTrainDetailReflection.Descriptor,
			TypeTrainInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(TrainRobFightResultAck), (MessageParser)(object)TrainRobFightResultAck.Parser, new string[5] { "TrainInfo", "RobTrain", "BattleSettle", "AwardList", "DataChange" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
