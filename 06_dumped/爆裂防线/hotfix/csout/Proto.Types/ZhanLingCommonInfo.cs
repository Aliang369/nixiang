using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Proto.Enums;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ZhanLingCommonInfo : IMessage<ZhanLingCommonInfo>, IMessage, IEquatable<ZhanLingCommonInfo>, IDeepCloneable<ZhanLingCommonInfo>, IBufferMessage
{
	private static readonly MessageParser<ZhanLingCommonInfo> _parser = new MessageParser<ZhanLingCommonInfo>((Func<ZhanLingCommonInfo>)(() => new ZhanLingCommonInfo()));

	private UnknownFieldSet _unknownFields;

	public const int ReachFieldNumber = 1;

	private int reach_;

	public const int ActiveFieldNumber = 2;

	private EBoolState active_;

	public const int GoodsIdFieldNumber = 3;

	private int goodsId_;

	public const int LowAwardFieldNumber = 4;

	private int lowAward_;

	public const int HighAwardFieldNumber = 5;

	private int highAward_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ZhanLingCommonInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeZhanLingCommonInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int Reach
	{
		get
		{
			return reach_;
		}
		set
		{
			reach_ = value;
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
	public int LowAward
	{
		get
		{
			return lowAward_;
		}
		set
		{
			lowAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public int HighAward
	{
		get
		{
			return highAward_;
		}
		set
		{
			highAward_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ZhanLingCommonInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ZhanLingCommonInfo(ZhanLingCommonInfo other)
		: this()
	{
		reach_ = other.reach_;
		active_ = other.active_;
		goodsId_ = other.goodsId_;
		lowAward_ = other.lowAward_;
		highAward_ = other.highAward_;
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ZhanLingCommonInfo Clone()
	{
		return new ZhanLingCommonInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ZhanLingCommonInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ZhanLingCommonInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (Reach != other.Reach)
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
		if (LowAward != other.LowAward)
		{
			return false;
		}
		if (HighAward != other.HighAward)
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
		if (Reach != 0)
		{
			num ^= Reach.GetHashCode();
		}
		if (Active != EBoolState.False)
		{
			num ^= Active.GetHashCode();
		}
		if (GoodsId != 0)
		{
			num ^= GoodsId.GetHashCode();
		}
		if (LowAward != 0)
		{
			num ^= LowAward.GetHashCode();
		}
		if (HighAward != 0)
		{
			num ^= HighAward.GetHashCode();
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
		if (Reach != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)8);
			((WriteContext)(ref output)).WriteInt32(Reach);
		}
		if (Active != EBoolState.False)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)16);
			((WriteContext)(ref output)).WriteEnum((int)Active);
		}
		if (GoodsId != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)24);
			((WriteContext)(ref output)).WriteInt32(GoodsId);
		}
		if (LowAward != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)32);
			((WriteContext)(ref output)).WriteInt32(LowAward);
		}
		if (HighAward != 0)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)40);
			((WriteContext)(ref output)).WriteInt32(HighAward);
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
		if (Reach != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(Reach);
		}
		if (Active != EBoolState.False)
		{
			num += 1 + CodedOutputStream.ComputeEnumSize((int)Active);
		}
		if (GoodsId != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(GoodsId);
		}
		if (LowAward != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(LowAward);
		}
		if (HighAward != 0)
		{
			num += 1 + CodedOutputStream.ComputeInt32Size(HighAward);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ZhanLingCommonInfo other)
	{
		if (other != null)
		{
			if (other.Reach != 0)
			{
				Reach = other.Reach;
			}
			if (other.Active != EBoolState.False)
			{
				Active = other.Active;
			}
			if (other.GoodsId != 0)
			{
				GoodsId = other.GoodsId;
			}
			if (other.LowAward != 0)
			{
				LowAward = other.LowAward;
			}
			if (other.HighAward != 0)
			{
				HighAward = other.HighAward;
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
				Reach = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 16u:
				Active = (EBoolState)((ParseContext)(ref input)).ReadEnum();
				break;
			case 24u:
				GoodsId = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 32u:
				LowAward = ((ParseContext)(ref input)).ReadInt32();
				break;
			case 40u:
				HighAward = ((ParseContext)(ref input)).ReadInt32();
				break;
			}
		}
	}
}
