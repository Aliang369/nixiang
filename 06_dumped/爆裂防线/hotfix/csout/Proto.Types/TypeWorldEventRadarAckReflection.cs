using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeWorldEventRadarAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeWorldEventRadarAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLldvcmxkRXZlbnRSYWRhckFjay5wcm90bxIFdHlwZXMaF3R5cGUu" + "SG9tZWxhbmRJbmZvLnByb3RvGhR0eXBlLldvcmxkSW5mby5wcm90byJkChJX" + "b3JsZEV2ZW50UmFkYXJBY2sSIwoJV29ybGRJbmZvGAEgASgLMhAudHlwZXMu" + "V29ybGRJbmZvEikKDEhvbWVsYW5kSW5mbxgCIAEoCzITLnR5cGVzLkhvbWVs" + "YW5kSW5mb0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[2]
		{
			TypeHomelandInfoReflection.Descriptor,
			TypeWorldInfoReflection.Descriptor
		}, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(WorldEventRadarAck), (MessageParser)(object)WorldEventRadarAck.Parser, new string[2] { "WorldInfo", "HomelandInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
