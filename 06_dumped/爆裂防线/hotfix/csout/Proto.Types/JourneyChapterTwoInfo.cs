using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class JourneyChapterTwoInfo : IMessage<JourneyChapterTwoInfo>, IMessage, IEquatable<JourneyChapterTwoInfo>, IDeepCloneable<JourneyChapterTwoInfo>, IBufferMessage
{
	private static readonly MessageParser<JourneyChapterTwoInfo> _parser = new MessageParser<JourneyChapterTwoInfo>((Func<JourneyChapterTwoInfo>)(() => new JourneyChapterTwoInfo()));

	private UnknownFieldSet _unknownFields;

	public const int BaseInfoFieldNumber = 1;

	private JourneyBaseInfo baseInfo_;

	public const int BarDrawFieldNumber = 2;

	private JourneyBarDrawDetail barDraw_;

	public const int DiamondCostFieldNumber = 3;

	private JourneyStatisticsDetail diamondCost_;

	public const int ExchangeFieldNumber = 4;

	private JourneyExchangeDetail exchange_;

	public const int ActiveFieldNumber = 5;

	private JourneyStatisticsDetail active_;

	public const int ZhanLingFieldNumber = 6;

	private JourneyZhanLingDetail zhanLing_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<JourneyChapterTwoInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeJourneyChapterTwoInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyBaseInfo BaseInfo
	{
		get
		{
			return baseInfo_;
		}
		set
		{
			baseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyBarDrawDetail BarDraw
	{
		get
		{
			return barDraw_;
		}
		set
		{
			barDraw_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyStatisticsDetail DiamondCost
	{
		get
		{
			return diamondCost_;
		}
		set
		{
			diamondCost_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyExchangeDetail Exchange
	{
		get
		{
			return exchange_;
		}
		set
		{
			exchange_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyStatisticsDetail Active
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
	public JourneyZhanLingDetail ZhanLing
	{
		get
		{
			return zhanLing_;
		}
		set
		{
			zhanLing_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyChapterTwoInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyChapterTwoInfo(JourneyChapterTwoInfo other)
		: this()
	{
		baseInfo_ = ((other.baseInfo_ != null) ? other.baseInfo_.Clone() : null);
		barDraw_ = ((other.barDraw_ != null) ? other.barDraw_.Clone() : null);
		diamondCost_ = ((other.diamondCost_ != null) ? other.diamondCost_.Clone() : null);
		exchange_ = ((other.exchange_ != null) ? other.exchange_.Clone() : null);
		active_ = ((other.active_ != null) ? other.active_.Clone() : null);
		zhanLing_ = ((other.zhanLing_ != null) ? other.zhanLing_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public JourneyChapterTwoInfo Clone()
	{
		return new JourneyChapterTwoInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as JourneyChapterTwoInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(JourneyChapterTwoInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(BaseInfo, other.BaseInfo))
		{
			return false;
		}
		if (!object.Equals(BarDraw, other.BarDraw))
		{
			return false;
		}
		if (!object.Equals(DiamondCost, other.DiamondCost))
		{
			return false;
		}
		if (!object.Equals(Exchange, other.Exchange))
		{
			return false;
		}
		if (!object.Equals(Active, other.Active))
		{
			return false;
		}
		if (!object.Equals(ZhanLing, other.ZhanLing))
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
		if (baseInfo_ != null)
		{
			num ^= BaseInfo.GetHashCode();
		}
		if (barDraw_ != null)
		{
			num ^= BarDraw.GetHashCode();
		}
		if (diamondCost_ != null)
		{
			num ^= DiamondCost.GetHashCode();
		}
		if (exchange_ != null)
		{
			num ^= Exchange.GetHashCode();
		}
		if (active_ != null)
		{
			num ^= Active.GetHashCode();
		}
		if (zhanLing_ != null)
		{
			num ^= ZhanLing.GetHashCode();
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
		if (baseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BaseInfo);
		}
		if (barDraw_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BarDraw);
		}
		if (diamondCost_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DiamondCost);
		}
		if (exchange_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Exchange);
		}
		if (active_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Active);
		}
		if (zhanLing_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)ZhanLing);
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
		if (baseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BaseInfo);
		}
		if (barDraw_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BarDraw);
		}
		if (diamondCost_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DiamondCost);
		}
		if (exchange_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Exchange);
		}
		if (active_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Active);
		}
		if (zhanLing_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)ZhanLing);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(JourneyChapterTwoInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.baseInfo_ != null)
		{
			if (baseInfo_ == null)
			{
				BaseInfo = new JourneyBaseInfo();
			}
			BaseInfo.MergeFrom(other.BaseInfo);
		}
		if (other.barDraw_ != null)
		{
			if (barDraw_ == null)
			{
				BarDraw = new JourneyBarDrawDetail();
			}
			BarDraw.MergeFrom(other.BarDraw);
		}
		if (other.diamondCost_ != null)
		{
			if (diamondCost_ == null)
			{
				DiamondCost = new JourneyStatisticsDetail();
			}
			DiamondCost.MergeFrom(other.DiamondCost);
		}
		if (other.exchange_ != null)
		{
			if (exchange_ == null)
			{
				Exchange = new JourneyExchangeDetail();
			}
			Exchange.MergeFrom(other.Exchange);
		}
		if (other.active_ != null)
		{
			if (active_ == null)
			{
				Active = new JourneyStatisticsDetail();
			}
			Active.MergeFrom(other.Active);
		}
		if (other.zhanLing_ != null)
		{
			if (zhanLing_ == null)
			{
				ZhanLing = new JourneyZhanLingDetail();
			}
			ZhanLing.MergeFrom(other.ZhanLing);
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
			case 10u:
				if (baseInfo_ == null)
				{
					BaseInfo = new JourneyBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BaseInfo);
				break;
			case 18u:
				if (barDraw_ == null)
				{
					BarDraw = new JourneyBarDrawDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BarDraw);
				break;
			case 26u:
				if (diamondCost_ == null)
				{
					DiamondCost = new JourneyStatisticsDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DiamondCost);
				break;
			case 34u:
				if (exchange_ == null)
				{
					Exchange = new JourneyExchangeDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Exchange);
				break;
			case 42u:
				if (active_ == null)
				{
					Active = new JourneyStatisticsDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Active);
				break;
			case 50u:
				if (zhanLing_ == null)
				{
					ZhanLing = new JourneyZhanLingDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)ZhanLing);
				break;
			}
		}
	}
}
