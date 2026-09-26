using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeNoviceLoginReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeNoviceLoginReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChZ0eXBlLk5vdmljZUxvZ2luLnByb3RvEgV0eXBlcxofdHlwZS5Ob3ZpY2VM" + "b2dpbkRheURldGFpbC5wcm90byJMCgtOb3ZpY2VMb2dpbhIPCgdTdGFydFRt" + "GAEgASgDEiwKB0RheUxpc3QYAiADKAsyGy50eXBlcy5Ob3ZpY2VMb2dpbkRh" + "eURldGFpbEIuWgkuLztwcm90b2OqAgtQcm90by5UeXBlc8oCElpiZ2FtZVxQ" + "cm90b1xUeXBlc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[1] { TypeNoviceLoginDayDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(NoviceLogin), (MessageParser)(object)NoviceLogin.Parser, new string[2] { "StartTm", "DayList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
