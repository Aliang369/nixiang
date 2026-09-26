using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class MineWarDataAck : IMessage<MineWarDataAck>, IMessage, IEquatable<MineWarDataAck>, IDeepCloneable<MineWarDataAck>, IBufferMessage
{
	private static readonly MessageParser<MineWarDataAck> _parser = new MessageParser<MineWarDataAck>((Func<MineWarDataAck>)(() => new MineWarDataAck()));

	private UnknownFieldSet _unknownFields;

	public const int ErrorCodeFieldNumber = 1;

	private EErrorCode errorCode_;

	public const int MineWarBaseInfoFieldNumber = 2;

	private MineWarBaseInfo mineWarBaseInfo_;

	public const int MineWarMapInfoFieldNumber = 3;

	private MineWarMapInfo mineWarMapInfo_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<MineWarDataAck> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeMineWarDataAckReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EErrorCode ErrorCode
	{
		get
		{
			return errorCode_;
		}
		set
		{
			errorCode_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarBaseInfo MineWarBaseInfo
	{
		get
		{
			return mineWarBaseInfo_;
		}
		set
		{
			mineWarBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarMapInfo MineWarMapInfo
	{
		get
		{
			return mineWarMapInfo_;
		}
		set
		{
			mineWarMapInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarDataAck()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarDataAck(MineWarDataAck other)
		: this()
	{
		errorCode_ = other.errorCode_;
		mineWarBaseInfo_ = ((other.mineWarBaseInfo_ != null) ? other.mineWarBaseInfo_.Clone() : null);
		mineWarMapInfo_ = ((other.mineWarMapInfo_ != null) ? other.mineWarMapInfo_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public MineWarDataAck Clone()
	{
		return new MineWarDataAck(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as MineWarDataAck);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(MineWarDataAck other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (ErrorCode != other.ErrorCode)
		{
			return false;
		}
		if (!object.Equals(MineWarBaseInfo, other.MineWarBaseInfo))
		{
			return false;
		}
		if (!object.Equals(MineWarMapInfo, other.MineWarMapInfo))
		{
			return false;
		}
		return object.Equals(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override int GetHashCode()
	{
		int num = 1;
		if (ErrorCode != EErrorCode.EmptyValue)
		{
			num ^= ErrorCode.GetHashCode();
		}
		if (mineWarBaseInfo_ != null)
		{
			num ^= MineWarBaseInfo.GetHashCode();
		}
		if (mineWarMapInfo_ != null)
		{
			num ^= MineWarMapInfo.GetHashCode();
		}
		if (_unknownFields != null)
		{
			num ^= ((object)_unknownFields).GetHashCode();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override string ToString()
	{
		return JsonFormatter.ToDiagnosticString((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void WriteTo(CodedOutputStream output)
	{
		output.WriteRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalWriteTo(ref WriteContext output)
	{
		if (ErrorCode != EErrorCode.EmptyValue)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteEnum((int)ErrorCode);
		}
		if (mineWarBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MineWarBaseInfo);
		}
		if (mineWarMapInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)MineWarMapInfo);
		}
		if (_unknownFields != null)
		{
			_unknownFields.WriteTo(ref output);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int CalculateSize()
	{
		int num = 0;
		if (ErrorCode != EErrorCode.EmptyValue)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)ErrorCode);
		}
		if (mineWarBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MineWarBaseInfo);
		}
		if (mineWarMapInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)MineWarMapInfo);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(MineWarDataAck other)
	{
		if (other == null)
		{
			return;
		}
		if (other.ErrorCode != EErrorCode.EmptyValue)
		{
			ErrorCode = other.ErrorCode;
		}
		if (other.mineWarBaseInfo_ != null)
		{
			if (mineWarBaseInfo_ == null)
			{
				MineWarBaseInfo = new MineWarBaseInfo();
			}
			MineWarBaseInfo.MergeFrom(other.MineWarBaseInfo);
		}
		if (other.mineWarMapInfo_ != null)
		{
			if (mineWarMapInfo_ == null)
			{
				MineWarMapInfo = new MineWarMapInfo();
			}
			MineWarMapInfo.MergeFrom(other.MineWarMapInfo);
		}
		_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(CodedInputStream input)
	{
		input.ReadRawMessage((IMessage)(object)this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	void IBufferMessage.InternalMergeFrom(ref ParseContext input)
	{
		uint num;
		while ((num = ((ParseContext)(ref input)).ReadTag()) != 0)
		{
			switch (num)
			{
			default:
				_unknownFields = UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
				break;
			case 8u:
				ErrorCode = (EErrorCode)((ParseContext)(ref input)).ReadEnum();
				break;
			case 18u:
				if (mineWarBaseInfo_ == null)
				{
					MineWarBaseInfo = new MineWarBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MineWarBaseInfo);
				break;
			case 26u:
				if (mineWarMapInfo_ == null)
				{
					MineWarMapInfo = new MineWarMapInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)MineWarMapInfo);
				break;
			}
		}
	}
}
