using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeCrossLadderUpgradeFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeCrossLadderUpgradeFightStartAckReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cip0eXBlLkNyb3NzTGFkZGVyVXBncmFkZUZpZ2h0U3RhcnRBY2sucHJvdG8S" + "BXR5cGVzGhV0eXBlLkJhdHRsZUluZm8ucHJvdG8iSAofQ3Jvc3NMYWRkZXJV" + "cGdyYWRlRmlnaHRTdGFydEFjaxIlCgpCYXR0bGVJbmZvGAEgASgLMhEudHlw" + "ZXMuQmF0dGxlSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpi" + "Z2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossLadderUpgradeFightStartAck), (MessageParser)(object)CrossLadderUpgradeFightStartAck.Parser, new string[1] { "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
