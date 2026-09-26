using System;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Enums;

public static class EnumEChannelTypeReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static EnumEChannelTypeReflection()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		descriptor = FileDescriptor.FromGeneratedCode(Convert.FromBase64String("ChdlbnVtLkVDaGFubmVsVHlwZS5wcm90bxIMRUNoYW5uZWxUeXBlKpICCgxF" + "Q2hhbm5lbFR5cGUSDgoKRW1wdHlWYWx1ZRAAEg0KB1Zpc2l0b3IQ4NQDEggK" + "AlpiEOHUAxIMCgZaYlNob3AQ4tQDEg0KB1piUm9ib3QQ49QDEgkKA0lvcxDx" + "ogQSDAoGVGFwVGFwEPKiBBIMCgZYaWFvTWkQ86IEEgwKBkh1YVdlaRD0ogQS" + "DQoHSGFybW9ueRD1ogQSDgoIQWxpR2FtZXMQ9qIEEgoKBFZpdm8Q96IEEgoK" + "BE9wcG8Q+KIEEgoKBEhZS0IQ+aIEEgoKBFRNR1AQ+qIEEgsKBU00Mzk5EPui" + "BBIOCghCaWxpYmlsaRD8ogQSDAoGRG91WWluEP2iBBINCgdEWVVuaW9uEP6i" + "BEIuWgkuLztwcm90b2OqAgtQcm90by5FbnVtc8oCElpiZ2FtZVxQcm90b1xF" + "bnVtc2IGcHJvdG8z"), (FileDescriptor[])(object)new FileDescriptor[0], new GeneratedClrTypeInfo(new Type[1] { typeof(EChannelType) }, (Extension[])null, (GeneratedClrTypeInfo[])null));
	}
}
