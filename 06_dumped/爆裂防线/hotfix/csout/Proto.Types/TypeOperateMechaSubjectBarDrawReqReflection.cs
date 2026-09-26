using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperateMechaSubjectBarDrawReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperateMechaSubjectBarDrawReqReflection()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Cih0eXBlLk9wZXJhdGVNZWNoYVN1YmplY3RCYXJEcmF3UmVxLnByb3RvEgV0" + "eXBlcyI8Ch1PcGVyYXRlTWVjaGFTdWJqZWN0QmFyRHJhd1JlcRIMCgRVVUlE" + "GAEgASgJEg0KBUNvdW50GAIgASgFQi5aCS4vO3Byb3RvY6oCC1Byb3RvLlR5" + "cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperateMechaSubjectBarDrawReq), (MessageParser)(object)OperateMechaSubjectBarDrawReq.Parser, new string[2] { "UUID", "Count" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
