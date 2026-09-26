using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeArenaFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeArenaFightStartAckReflection()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkFyZW5hRmlnaHRTdGFydEFjay5wcm90bxIFdHlwZXMaFXR5cGUu" + "QmF0dGxlSW5mby5wcm90byJbChJBcmVuYUZpZ2h0U3RhcnRBY2sSEAoIUGxh" + "eWVySWQYASABKAUSDAoEUmFuaxgCIAEoBRIlCgpCYXR0bGVJbmZvGAMgASgL" + "MhEudHlwZXMuQmF0dGxlSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBl" + "c8oCElpiZ2FtZVxQcm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(ArenaFightStartAck), (MessageParser)(object)ArenaFightStartAck.Parser, new string[3] { "PlayerId", "Rank", "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
