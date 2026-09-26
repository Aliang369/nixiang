using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderInfoReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderInfoReflection()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Chp0eXBlLkNyb3NzTGFkZGVySW5mby5wcm90bxIFdHlwZXMaIXR5cGUuQ3Jv" + "c3NMYWRkZXJBY2hpZXZlSW5mby5wcm90bxogdHlwZS5Dcm9zc0xhZGRlckNv" + "bWJhdEluZm8ucHJvdG8aHnR5cGUuQ3Jvc3NMYWRkZXJPcGVuSW5mby5wcm90" + "bxoedHlwZS5Dcm9zc0xhZGRlclNlbGZJbmZvLnByb3RvItMBCg9Dcm9zc0xh" + "ZGRlckluZm8SLAoIT3BlbkluZm8YASABKAsyGi50eXBlcy5Dcm9zc0xhZGRl" + "ck9wZW5JbmZvEiwKCFNlbGZJbmZvGAIgASgLMhoudHlwZXMuQ3Jvc3NMYWRk" + "ZXJTZWxmSW5mbxIyCgtBY2hpZXZlSW5mbxgDIAEoCzIdLnR5cGVzLkNyb3Nz" + "TGFkZGVyQWNoaWV2ZUluZm8SMAoKQ29tYmF0SW5mbxgEIAEoCzIcLnR5cGVz" + "LkNyb3NzTGFkZGVyQ29tYmF0SW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5U" + "eXBlc8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[4]
		{
			TypeCrossLadderAchieveInfoReflection.Descriptor,
			TypeCrossLadderCombatInfoReflection.Descriptor,
			TypeCrossLadderOpenInfoReflection.Descriptor,
			TypeCrossLadderSelfInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderInfo), (MessageParser)(object)CrossLadderInfo.Parser, new string[4] { "OpenInfo", "SelfInfo", "AchieveInfo", "CombatInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
