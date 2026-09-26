using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateTacticRouletteSelectAckReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateTacticRouletteSelectAckReflection()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cil0eXBlLk9wZXJhdGVUYWN0aWNSb3VsZXR0ZVNlbGVjdEFjay5wcm90bxIF" + "dHlwZXMaH3R5cGUuVGFjdGljUm91bGV0dGVEZXRhaWwucHJvdG8iWQoeT3Bl" + "cmF0ZVRhY3RpY1JvdWxldHRlU2VsZWN0QWNrEjcKElRhY3RpY1JvdWxldHRl" + "TGlzdBgBIAMoCzIbLnR5cGVzLlRhY3RpY1JvdWxldHRlRGV0YWlsQi5aCS4v" + "O3Byb3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZw" + "cm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeTacticRouletteDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateTacticRouletteSelectAck), (MessageParser)(object)OperateTacticRouletteSelectAck.Parser, new string[1] { "TacticRouletteList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
