using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

public static class TypeAuxExchangeHalfReqReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static TypeAuxExchangeHalfReqReflection()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("Ch10eXBlLkF1eEV4Y2hhbmdlSGFsZlJlcS5wcm90bxIFdHlwZXMaFWVudW0u" + "RU1vbmV5VHlwZS5wcm90byI/ChJBdXhFeGNoYW5nZUhhbGZSZXESKQoJTW9u" + "ZXlUeXBlGAEgASgOMhYuRU1vbmV5VHlwZS5FTW9uZXlUeXBlQi5aCS4vO3By" + "b3RvY6oCC1Byb3RvLlR5cGVzygISWmJnYW1lXFByb3RvXFR5cGVzYgZwcm90" + "bzM="), (FileDescriptor[])(object)new FileDescriptor[1] { EnumEMoneyTypeReflection.Descriptor }, new GeneratedClrTypeInfo((Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])(object)new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(AuxExchangeHalfReq), (MessageParser)(object)AuxExchangeHalfReq.Parser, new string[1] { "MoneyType" }, (string[])null, (Type[])null, (Extension[])null, (GeneratedClrTypeInfo[])null)
		}));
	}
}
