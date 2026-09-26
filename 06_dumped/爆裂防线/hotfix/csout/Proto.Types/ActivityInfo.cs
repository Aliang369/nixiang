using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class ActivityInfo : IMessage<ActivityInfo>, IMessage, IEquatable<ActivityInfo>, IDeepCloneable<ActivityInfo>, IBufferMessage
{
	private static readonly MessageParser<ActivityInfo> _parser = new MessageParser<ActivityInfo>((Func<ActivityInfo>)(() => new ActivityInfo()));

	private UnknownFieldSet _unknownFields;

	public const int NoviceLoginFieldNumber = 1;

	private NoviceLogin noviceLogin_;

	public const int DailySignFieldNumber = 2;

	private DailySign dailySign_;

	public const int TiliBankFieldNumber = 3;

	private TiliBank tiliBank_;

	public const int BubbleGiftFieldNumber = 4;

	private BubbleGift bubbleGift_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<ActivityInfo> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeActivityInfoReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public NoviceLogin NoviceLogin
	{
		get
		{
			return noviceLogin_;
		}
		set
		{
			noviceLogin_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DailySign DailySign
	{
		get
		{
			return dailySign_;
		}
		set
		{
			dailySign_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TiliBank TiliBank
	{
		get
		{
			return tiliBank_;
		}
		set
		{
			tiliBank_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public BubbleGift BubbleGift
	{
		get
		{
			return bubbleGift_;
		}
		set
		{
			bubbleGift_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ActivityInfo()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ActivityInfo(ActivityInfo other)
		: this()
	{
		noviceLogin_ = ((other.noviceLogin_ != null) ? other.noviceLogin_.Clone() : null);
		dailySign_ = ((other.dailySign_ != null) ? other.dailySign_.Clone() : null);
		tiliBank_ = ((other.tiliBank_ != null) ? other.tiliBank_.Clone() : null);
		bubbleGift_ = ((other.bubbleGift_ != null) ? other.bubbleGift_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ActivityInfo Clone()
	{
		return new ActivityInfo(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as ActivityInfo);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(ActivityInfo other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(NoviceLogin, other.NoviceLogin))
		{
			return false;
		}
		if (!object.Equals(DailySign, other.DailySign))
		{
			return false;
		}
		if (!object.Equals(TiliBank, other.TiliBank))
		{
			return false;
		}
		if (!object.Equals(BubbleGift, other.BubbleGift))
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
		if (noviceLogin_ != null)
		{
			num ^= NoviceLogin.GetHashCode();
		}
		if (dailySign_ != null)
		{
			num ^= DailySign.GetHashCode();
		}
		if (tiliBank_ != null)
		{
			num ^= TiliBank.GetHashCode();
		}
		if (bubbleGift_ != null)
		{
			num ^= BubbleGift.GetHashCode();
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
		if (noviceLogin_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)NoviceLogin);
		}
		if (dailySign_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)DailySign);
		}
		if (tiliBank_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TiliBank);
		}
		if (bubbleGift_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)BubbleGift);
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
		if (noviceLogin_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)NoviceLogin);
		}
		if (dailySign_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)DailySign);
		}
		if (tiliBank_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TiliBank);
		}
		if (bubbleGift_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)BubbleGift);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(ActivityInfo other)
	{
		if (other == null)
		{
			return;
		}
		if (other.noviceLogin_ != null)
		{
			if (noviceLogin_ == null)
			{
				NoviceLogin = new NoviceLogin();
			}
			NoviceLogin.MergeFrom(other.NoviceLogin);
		}
		if (other.dailySign_ != null)
		{
			if (dailySign_ == null)
			{
				DailySign = new DailySign();
			}
			DailySign.MergeFrom(other.DailySign);
		}
		if (other.tiliBank_ != null)
		{
			if (tiliBank_ == null)
			{
				TiliBank = new TiliBank();
			}
			TiliBank.MergeFrom(other.TiliBank);
		}
		if (other.bubbleGift_ != null)
		{
			if (bubbleGift_ == null)
			{
				BubbleGift = new BubbleGift();
			}
			BubbleGift.MergeFrom(other.BubbleGift);
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
				if (noviceLogin_ == null)
				{
					NoviceLogin = new NoviceLogin();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)NoviceLogin);
				break;
			case 18u:
				if (dailySign_ == null)
				{
					DailySign = new DailySign();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)DailySign);
				break;
			case 26u:
				if (tiliBank_ == null)
				{
					TiliBank = new TiliBank();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TiliBank);
				break;
			case 34u:
				if (bubbleGift_ == null)
				{
					BubbleGift = new BubbleGift();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)BubbleGift);
				break;
			}
		}
	}
}
