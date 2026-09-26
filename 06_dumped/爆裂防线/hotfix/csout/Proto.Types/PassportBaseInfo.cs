using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class PassportBaseInfo : IMessage<PassportBaseInfo>, IMessage, IEquatable<PassportBaseInfo>, IDeepCloneable<PassportBaseInfo>, IBufferMessage
{
	private static readonly MessageParser<PassportBaseInfo> _parser = new MessageParser<PassportBaseInfo>((Func<PassportBaseInfo>)(() => new PassportBaseInfo()));

	private UnknownFieldSet _unknownFields;

	public const int StartTmFieldNumber = 1;

	private long startTm_;

	public const int EndTmFieldNumber = 2;

	private long endTm_;

	public const int ActiveFieldNumber = 3;

	private EBoolState active_;

	public const int GoodsIdFieldNumber = 4;

	private int goodsId_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<PassportBaseInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypePassportBaseInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long StartTm
	{
		get
		{
			return startTm_;
		}
		set
		{
			startTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public long EndTm
	{
		get
		{
			return endTm_;
		}
		set
		{
			endTm_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EBoolState Active
	{
		get
		{
			return active_;
		}
		set
		{
			active_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int GoodsId
	{
		get
		{
			return goodsId_;
		}
		set
		{
			goodsId_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PassportBaseInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PassportBaseInfo(PassportBaseInfo other)
		: this()
	{
		startTm_ = other.startTm_;
		endTm_ = other.endTm_;
		active_ = other.active_;
		goodsId_ = other.goodsId_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PassportBaseInfo Clone()
	{
		return new PassportBaseInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as PassportBaseInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(PassportBaseInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (StartTm != other.StartTm)
		{
			return false;
		}
		if (EndTm != other.EndTm)
		{
			return false;
		}
		if (Active != other.Active)
		{
			return false;
		}
		if (GoodsId != other.GoodsId)
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
		if (StartTm != 0L)
		{
			num ^= StartTm.GetHashCode();
		}
		if (EndTm != 0L)
		{
			num ^= EndTm.GetHashCode();
		}
		if (Active != EBoolState.False)
		{
			num ^= Active.GetHashCode();
		}
		if (GoodsId != 0)
		{
			num ^= GoodsId.GetHashCode();
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
		if (StartTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt64(StartTm);
		}
		if (EndTm != 0L)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteInt64(EndTm);
		}
		if (Active != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteEnum((int)Active);
		}
		if (GoodsId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(GoodsId);
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
		if (StartTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(StartTm);
		}
		if (EndTm != 0L)
		{
			num += 1 + CodedOutputStream.ComputeInt64Size(EndTm);
		}
		if (Active != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Active);
		}
		if (GoodsId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GoodsId);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(PassportBaseInfo other)
	{
		if (other != null)
		{
			if (other.StartTm != 0L)
			{
				StartTm = other.StartTm;
			}
			if (other.EndTm != 0L)
			{
				EndTm = other.EndTm;
			}
			if (other.Active != EBoolState.False)
			{
				Active = other.Active;
			}
			if (other.GoodsId != 0)
			{
				GoodsId = other.GoodsId;
			}
			_unknownFields = UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
		}
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
				StartTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 16u:
				EndTm = ((ParseContext)(ref input)).ReadInt64();
				break;
			case 24u:
				Active = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 32u:
				GoodsId = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
