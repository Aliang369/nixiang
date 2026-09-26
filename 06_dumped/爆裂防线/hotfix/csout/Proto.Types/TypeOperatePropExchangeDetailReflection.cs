using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

public static class TypeOperatePropExchangeDetailReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeOperatePropExchangeDetailReflection()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("CiR0eXBlLk9wZXJhdGVQcm9wRXhjaGFuZ2VEZXRhaWwucHJvdG8SBXR5cGVz" + "GhZ0eXBlLkF3YXJkRGV0YWlsLnByb3RvIoQBChlPcGVyYXRlUHJvcEV4Y2hh" + "bmdlRGV0YWlsEg0KBUluZGV4GAEgASgFEhAKCFByb3BDb3N0GAIgASgFEhAK" + "CExpbWl0TnVtGAMgASgFEg0KBUNvdW50GAQgASgFEiUKCUF3YXJkTGlzdBgF" + "IAMoCzISLnR5cGVzLkF3YXJkRGV0YWlsQi5aCS4vO3Byb3RvY6oCC1Byb3Rv" + "LlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { TypeAwardDetailReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(OperatePropExchangeDetail), (MessageParser)(object)OperatePropExchangeDetail.Parser, new string[5] { "Index", "PropCost", "LimitNum", "Count", "AwardList" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
