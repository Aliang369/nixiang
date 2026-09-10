using System.Collections.Generic;
using FMODUnity;

public class AudioData : ScriptableObjectSingleton<AudioData>
{
	[ParamRef]
	public string mainVolume;

	[ParamRef]
	public string bGMVolume;

	[ParamRef]
	public string soundEffectVolume;

	[BankRef]
	public List<string> GlobalLoadBanks;

	[BankRef]
	public List<string> FortressBanks;

	[BankRef]
	public List<string> InGameBanks;

	public EventReference bgm_general_battle_elite;

	public EventReference bgm_general_battle_normal;

	public EventReference bgm_menu_fortress;

	public EventReference bgm_menu_login;

	public EventReference bgm_gameover_lose;

	public EventReference bgm_gameover_win;

	public EventReference sfx_ambience_blacksmith;

	public EventReference sfx_ambience_bonfire;

	public EventReference sfx_ambience_store;

	public EventReference sfx_ambience_crystal_upgrade;

	public EventReference sfx_ambience_abyssal_stone;

	public EventReference sfx_click_general;

	public EventReference sfx_click_secondary;

	public EventReference sfx_click_tab;

	public EventReference sfx_hint_pop_up;

	public EventReference sfx_item_acquire;

	public EventReference sfx_item_acquire_rare;

	public EventReference sfx_item_acquire_epic;

	public EventReference sfx_click_nagative;

	public EventReference sfx_gameplay_amulet_activate;

	public EventReference sfx_gameplay_amulet_click;

	public EventReference sfx_gameplay_amulet_trigger;

	public EventReference sfx_gameplay_bless_acquire;

	public EventReference sfx_gameplay_card_cast;

	public EventReference sfx_gameplay_card_destroy;

	public EventReference sfx_gameplay_card_discard;

	public EventReference sfx_gameplay_card_drag;

	public EventReference sfx_gameplay_card_exchange;

	public EventReference sfx_gameplay_card_gather;

	public EventReference sfx_gameplay_card_over;

	public EventReference sfx_gameplay_card_runed;

	public EventReference sfx_gameplay_card_select;

	public EventReference sfx_gameplay_card_shuffle;

	public EventReference sfx_gameplay_card_upgrade;

	public EventReference sfx_gameplay_death_pending;

	public EventReference sfx_gameplay_death_stinger;

	public EventReference sfx_gameplay_hit_default;

	public EventReference sfx_gameplay_defence_hit;

	public EventReference sfx_gameplay_defence_hit_break;

	public EventReference sfx_gameplay_defence_hit_final_break;

	public EventReference sfx_gameplay_enchant_buff;

	public EventReference sfx_gameplay_enchant_debuff;

	public EventReference sfx_gameplay_enter_blessed_root;

	public EventReference sfx_gameplay_enter_pioneer;

	public EventReference sfx_gameplay_enter_shop;

	public EventReference sfx_gameplay_event_choice_confirm;

	public EventReference sfx_gameplay_event_choice_select;

	public EventReference sfx_gameplay_foe_card_cast;

	public EventReference sfx_gameplay_foe_card_draw;

	public EventReference sfx_gameplay_foe_crystal_activated;

	public EventReference sfx_gameplay_foe_crystal_deactivated;

	public EventReference sfx_gameplay_heal;

	public EventReference sfx_gameplay_map_confirm;

	public EventReference sfx_gameplay_map_movement;

	public EventReference sfx_gameplay_map_pindown;

	public EventReference sfx_gameplay_player_card_draw;

	public EventReference sfx_gameplay_resources_coin_change;

	public EventReference sfx_gameplay_resources_crystal_change;

	public EventReference sfx_gameplay_scroll;

	public EventReference sfx_gameplay_scroll_click;

	public EventReference sfx_gameplay_turn_over;

	public EventReference sfx_gameplay_victory_stinger;

	public EventReference sfx_gameplay_win_boss_fight_hint;

	public EventReference sfx_gameplay_turn_banner_player;

	public EventReference sfx_gameplay_turn_banner_enemy;

	public EventReference sfx_menu_altar_activate;

	public EventReference sfx_menu_altar_enter;

	public EventReference sfx_menu_amulet_delete;

	public EventReference sfx_menu_amulet_enchant;

	public EventReference sfx_menu_dream_credit;

	public EventReference sfx_menu_dream_target;

	public EventReference sfx_menu_gacha_draw;

	public EventReference sfx_menu_gacha_draw_rare;

	public EventReference sfx_menu_gacha_draw_epic;

	public EventReference sfx_menu_gacha_draw_crystal;

	public EventReference sfx_menu_amulet_affixes;

	public EventReference sfx_menu_amulet_slot;

	public EventReference sfx_menu_purchase_click;

	public EventReference sfx_bonfire_out;

	public EventReference sfx_menu_stargaze_constellation_select;

	public EventReference sfx_menu_stargaze_star_select;

	public EventReference sfx_menu_unlock_difficulty_clavis;

	public EventReference sfx_menu_switch_layer;

	public EventReference sfx_menu_talents_upgrade;

	public EventReference sfx_menu_treasure_open;

	public EventReference sfx_transition;

	public EventReference sfx_menu_hero_gather;

	public EventReference sfx_menu_gem_mount_move;

	public EventReference sfx_menu_gem_combine;

	public EventReference sfx_menu_chracter_obsession_level_add;

	public EventReference sfx_menu_amulet_gem_slot;

	public EventReference sfx_menu_amulet_breakthrough;
}
