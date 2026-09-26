using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Proto.Types;

[DebuggerDisplay("{ToString(),nq}")]
public sealed class DataChange : IMessage<DataChange>, IMessage, IEquatable<DataChange>, IDeepCloneable<DataChange>, IBufferMessage
{
	private static readonly MessageParser<DataChange> _parser = new MessageParser<DataChange>((Func<DataChange>)(() => new DataChange()));

	private UnknownFieldSet _unknownFields;

	public const int RoleBaseInfoFieldNumber = 1;

	private RoleBaseInfo roleBaseInfo_;

	public const int RoleCommonInfoFieldNumber = 2;

	private RoleCommonInfo roleCommonInfo_;

	public const int RoleTeamInfoFieldNumber = 3;

	private RoleTeamInfo roleTeamInfo_;

	public const int MoneyFieldNumber = 4;

	private ResourceChangeDetail money_;

	public const int ItemFieldNumber = 5;

	private ResourceChangeDetail item_;

	public const int HeroFragFieldNumber = 6;

	private ResourceChangeDetail heroFrag_;

	public const int BoxFieldNumber = 7;

	private ResourceChangeDetail box_;

	public const int GemFieldNumber = 8;

	private ResourceChangeDetail gem_;

	public const int PropFieldNumber = 9;

	private PropChangeDetail prop_;

	public const int EquipFieldNumber = 10;

	private EquipChangeDetail equip_;

	public const int HeroFieldNumber = 11;

	private HeroChangeDetail hero_;

	public const int GeneFieldNumber = 12;

	private ResourceChangeDetail gene_;

	public const int TacticFragFieldNumber = 13;

	private ResourceChangeDetail tacticFrag_;

	public const int TacticFieldNumber = 14;

	private TacticChangeDetail tactic_;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageParser<DataChange> Parser => _parser;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public static MessageDescriptor Descriptor => TypeDataChangeReflection.Descriptor.MessageTypes[0];

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	MessageDescriptor IMessage.Descriptor => Descriptor;

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleBaseInfo RoleBaseInfo
	{
		get
		{
			return roleBaseInfo_;
		}
		set
		{
			roleBaseInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleCommonInfo RoleCommonInfo
	{
		get
		{
			return roleCommonInfo_;
		}
		set
		{
			roleCommonInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public RoleTeamInfo RoleTeamInfo
	{
		get
		{
			return roleTeamInfo_;
		}
		set
		{
			roleTeamInfo_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ResourceChangeDetail Money
	{
		get
		{
			return money_;
		}
		set
		{
			money_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ResourceChangeDetail Item
	{
		get
		{
			return item_;
		}
		set
		{
			item_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ResourceChangeDetail HeroFrag
	{
		get
		{
			return heroFrag_;
		}
		set
		{
			heroFrag_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ResourceChangeDetail Box
	{
		get
		{
			return box_;
		}
		set
		{
			box_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ResourceChangeDetail Gem
	{
		get
		{
			return gem_;
		}
		set
		{
			gem_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public PropChangeDetail Prop
	{
		get
		{
			return prop_;
		}
		set
		{
			prop_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public EquipChangeDetail Equip
	{
		get
		{
			return equip_;
		}
		set
		{
			equip_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public HeroChangeDetail Hero
	{
		get
		{
			return hero_;
		}
		set
		{
			hero_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ResourceChangeDetail Gene
	{
		get
		{
			return gene_;
		}
		set
		{
			gene_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public ResourceChangeDetail TacticFrag
	{
		get
		{
			return tacticFrag_;
		}
		set
		{
			tacticFrag_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public TacticChangeDetail Tactic
	{
		get
		{
			return tactic_;
		}
		set
		{
			tactic_ = value;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DataChange()
	{
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DataChange(DataChange other)
		: this()
	{
		roleBaseInfo_ = ((other.roleBaseInfo_ != null) ? other.roleBaseInfo_.Clone() : null);
		roleCommonInfo_ = ((other.roleCommonInfo_ != null) ? other.roleCommonInfo_.Clone() : null);
		roleTeamInfo_ = ((other.roleTeamInfo_ != null) ? other.roleTeamInfo_.Clone() : null);
		money_ = ((other.money_ != null) ? other.money_.Clone() : null);
		item_ = ((other.item_ != null) ? other.item_.Clone() : null);
		heroFrag_ = ((other.heroFrag_ != null) ? other.heroFrag_.Clone() : null);
		box_ = ((other.box_ != null) ? other.box_.Clone() : null);
		gem_ = ((other.gem_ != null) ? other.gem_.Clone() : null);
		prop_ = ((other.prop_ != null) ? other.prop_.Clone() : null);
		equip_ = ((other.equip_ != null) ? other.equip_.Clone() : null);
		hero_ = ((other.hero_ != null) ? other.hero_.Clone() : null);
		gene_ = ((other.gene_ != null) ? other.gene_.Clone() : null);
		tacticFrag_ = ((other.tacticFrag_ != null) ? other.tacticFrag_.Clone() : null);
		tactic_ = ((other.tactic_ != null) ? other.tactic_.Clone() : null);
		_unknownFields = UnknownFieldSet.Clone(other._unknownFields);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public DataChange Clone()
	{
		return new DataChange(this);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public override bool Equals(object other)
	{
		return Equals(other as DataChange);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public bool Equals(DataChange other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (!object.Equals(RoleBaseInfo, other.RoleBaseInfo))
		{
			return false;
		}
		if (!object.Equals(RoleCommonInfo, other.RoleCommonInfo))
		{
			return false;
		}
		if (!object.Equals(RoleTeamInfo, other.RoleTeamInfo))
		{
			return false;
		}
		if (!object.Equals(Money, other.Money))
		{
			return false;
		}
		if (!object.Equals(Item, other.Item))
		{
			return false;
		}
		if (!object.Equals(HeroFrag, other.HeroFrag))
		{
			return false;
		}
		if (!object.Equals(Box, other.Box))
		{
			return false;
		}
		if (!object.Equals(Gem, other.Gem))
		{
			return false;
		}
		if (!object.Equals(Prop, other.Prop))
		{
			return false;
		}
		if (!object.Equals(Equip, other.Equip))
		{
			return false;
		}
		if (!object.Equals(Hero, other.Hero))
		{
			return false;
		}
		if (!object.Equals(Gene, other.Gene))
		{
			return false;
		}
		if (!object.Equals(TacticFrag, other.TacticFrag))
		{
			return false;
		}
		if (!object.Equals(Tactic, other.Tactic))
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
		if (roleBaseInfo_ != null)
		{
			num ^= RoleBaseInfo.GetHashCode();
		}
		if (roleCommonInfo_ != null)
		{
			num ^= RoleCommonInfo.GetHashCode();
		}
		if (roleTeamInfo_ != null)
		{
			num ^= RoleTeamInfo.GetHashCode();
		}
		if (money_ != null)
		{
			num ^= Money.GetHashCode();
		}
		if (item_ != null)
		{
			num ^= Item.GetHashCode();
		}
		if (heroFrag_ != null)
		{
			num ^= HeroFrag.GetHashCode();
		}
		if (box_ != null)
		{
			num ^= Box.GetHashCode();
		}
		if (gem_ != null)
		{
			num ^= Gem.GetHashCode();
		}
		if (prop_ != null)
		{
			num ^= Prop.GetHashCode();
		}
		if (equip_ != null)
		{
			num ^= Equip.GetHashCode();
		}
		if (hero_ != null)
		{
			num ^= Hero.GetHashCode();
		}
		if (gene_ != null)
		{
			num ^= Gene.GetHashCode();
		}
		if (tacticFrag_ != null)
		{
			num ^= TacticFrag.GetHashCode();
		}
		if (tactic_ != null)
		{
			num ^= Tactic.GetHashCode();
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
		if (roleBaseInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)10);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleBaseInfo);
		}
		if (roleCommonInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)18);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleCommonInfo);
		}
		if (roleTeamInfo_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)26);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)RoleTeamInfo);
		}
		if (money_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)34);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Money);
		}
		if (item_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)42);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Item);
		}
		if (heroFrag_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)50);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)HeroFrag);
		}
		if (box_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)58);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Box);
		}
		if (gem_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)66);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Gem);
		}
		if (prop_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)74);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Prop);
		}
		if (equip_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)82);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Equip);
		}
		if (hero_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)90);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Hero);
		}
		if (gene_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)98);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Gene);
		}
		if (tacticFrag_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)106);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)TacticFrag);
		}
		if (tactic_ != null)
		{
			((WriteContext)(ref output)).WriteRawTag((byte)114);
			((WriteContext)(ref output)).WriteMessage((IMessage)(object)Tactic);
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
		if (roleBaseInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleBaseInfo);
		}
		if (roleCommonInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleCommonInfo);
		}
		if (roleTeamInfo_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)RoleTeamInfo);
		}
		if (money_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Money);
		}
		if (item_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Item);
		}
		if (heroFrag_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)HeroFrag);
		}
		if (box_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Box);
		}
		if (gem_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Gem);
		}
		if (prop_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Prop);
		}
		if (equip_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Equip);
		}
		if (hero_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Hero);
		}
		if (gene_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Gene);
		}
		if (tacticFrag_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)TacticFrag);
		}
		if (tactic_ != null)
		{
			num += 1 + CodedOutputStream.ComputeMessageSize((IMessage)(object)Tactic);
		}
		if (_unknownFields != null)
		{
			num += _unknownFields.CalculateSize();
		}
		return num;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("protoc", null)]
	public void MergeFrom(DataChange other)
	{
		if (other == null)
		{
			return;
		}
		if (other.roleBaseInfo_ != null)
		{
			if (roleBaseInfo_ == null)
			{
				RoleBaseInfo = new RoleBaseInfo();
			}
			RoleBaseInfo.MergeFrom(other.RoleBaseInfo);
		}
		if (other.roleCommonInfo_ != null)
		{
			if (roleCommonInfo_ == null)
			{
				RoleCommonInfo = new RoleCommonInfo();
			}
			RoleCommonInfo.MergeFrom(other.RoleCommonInfo);
		}
		if (other.roleTeamInfo_ != null)
		{
			if (roleTeamInfo_ == null)
			{
				RoleTeamInfo = new RoleTeamInfo();
			}
			RoleTeamInfo.MergeFrom(other.RoleTeamInfo);
		}
		if (other.money_ != null)
		{
			if (money_ == null)
			{
				Money = new ResourceChangeDetail();
			}
			Money.MergeFrom(other.Money);
		}
		if (other.item_ != null)
		{
			if (item_ == null)
			{
				Item = new ResourceChangeDetail();
			}
			Item.MergeFrom(other.Item);
		}
		if (other.heroFrag_ != null)
		{
			if (heroFrag_ == null)
			{
				HeroFrag = new ResourceChangeDetail();
			}
			HeroFrag.MergeFrom(other.HeroFrag);
		}
		if (other.box_ != null)
		{
			if (box_ == null)
			{
				Box = new ResourceChangeDetail();
			}
			Box.MergeFrom(other.Box);
		}
		if (other.gem_ != null)
		{
			if (gem_ == null)
			{
				Gem = new ResourceChangeDetail();
			}
			Gem.MergeFrom(other.Gem);
		}
		if (other.prop_ != null)
		{
			if (prop_ == null)
			{
				Prop = new PropChangeDetail();
			}
			Prop.MergeFrom(other.Prop);
		}
		if (other.equip_ != null)
		{
			if (equip_ == null)
			{
				Equip = new EquipChangeDetail();
			}
			Equip.MergeFrom(other.Equip);
		}
		if (other.hero_ != null)
		{
			if (hero_ == null)
			{
				Hero = new HeroChangeDetail();
			}
			Hero.MergeFrom(other.Hero);
		}
		if (other.gene_ != null)
		{
			if (gene_ == null)
			{
				Gene = new ResourceChangeDetail();
			}
			Gene.MergeFrom(other.Gene);
		}
		if (other.tacticFrag_ != null)
		{
			if (tacticFrag_ == null)
			{
				TacticFrag = new ResourceChangeDetail();
			}
			TacticFrag.MergeFrom(other.TacticFrag);
		}
		if (other.tactic_ != null)
		{
			if (tactic_ == null)
			{
				Tactic = new TacticChangeDetail();
			}
			Tactic.MergeFrom(other.Tactic);
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
				if (roleBaseInfo_ == null)
				{
					RoleBaseInfo = new RoleBaseInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleBaseInfo);
				break;
			case 18u:
				if (roleCommonInfo_ == null)
				{
					RoleCommonInfo = new RoleCommonInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleCommonInfo);
				break;
			case 26u:
				if (roleTeamInfo_ == null)
				{
					RoleTeamInfo = new RoleTeamInfo();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)RoleTeamInfo);
				break;
			case 34u:
				if (money_ == null)
				{
					Money = new ResourceChangeDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Money);
				break;
			case 42u:
				if (item_ == null)
				{
					Item = new ResourceChangeDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Item);
				break;
			case 50u:
				if (heroFrag_ == null)
				{
					HeroFrag = new ResourceChangeDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)HeroFrag);
				break;
			case 58u:
				if (box_ == null)
				{
					Box = new ResourceChangeDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Box);
				break;
			case 66u:
				if (gem_ == null)
				{
					Gem = new ResourceChangeDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Gem);
				break;
			case 74u:
				if (prop_ == null)
				{
					Prop = new PropChangeDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Prop);
				break;
			case 82u:
				if (equip_ == null)
				{
					Equip = new EquipChangeDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Equip);
				break;
			case 90u:
				if (hero_ == null)
				{
					Hero = new HeroChangeDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Hero);
				break;
			case 98u:
				if (gene_ == null)
				{
					Gene = new ResourceChangeDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Gene);
				break;
			case 106u:
				if (tacticFrag_ == null)
				{
					TacticFrag = new ResourceChangeDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)TacticFrag);
				break;
			case 114u:
				if (tactic_ == null)
				{
					Tactic = new TacticChangeDetail();
				}
				((ParseContext)(ref input)).ReadMessage((IMessage)(object)Tactic);
				break;
			}
		}
	}
}
