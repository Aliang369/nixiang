using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateMechaSubjectFightStartAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateMechaSubjectFightStartAckReflection()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cit0eXBlLk9wZXJhdGVNZWNoYVN1YmplY3RGaWdodFN0YXJ0QWNrLnByb3Rv" + "EgV0eXBlcxoVdHlwZS5CYXR0bGVJbmZvLnByb3RvImgKIE9wZXJhdGVNZWNo" + "YVN1YmplY3RGaWdodFN0YXJ0QWNrEgwKBFVVSUQYASABKAkSDwoHTGV2ZWxJ" + "ZBgCIAEoBRIlCgpCYXR0bGVJbmZvGAMgASgLMhEudHlwZXMuQmF0dGxlSW5m" + "b0IuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQcm90b1xU" + "eXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeBattleInfoReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateMechaSubjectFightStartAck), (MessageParser)(object)OperateMechaSubjectFightStartAck.Parser, new string[3] { "UUID", "LevelId", "BattleInfo" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
