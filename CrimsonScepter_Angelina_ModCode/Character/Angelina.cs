using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Cards;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Extensions;
using CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace CrimsonScepter_Angelina_Mod.CrimsonScepter_Angelina_ModCode.Character;

public class Angelina : PlaceholderCharacterModel
{
    public const string CharacterId = "Angelina";                       // 定义角色ID
    public static readonly Color Color = new("6CB8F6");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 70;                               //起始血量 

    // 定义起始卡组
    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeAngelina>(),
        ModelDb.Card<StrikeAngelina>(),
        ModelDb.Card<StrikeAngelina>(),
        ModelDb.Card<StrikeAngelina>(),
        ModelDb.Card<DefendAngelina>(),
        ModelDb.Card<DefendAngelina>(),
        ModelDb.Card<DefendAngelina>(),
        ModelDb.Card<DefendAngelina>(),
        ModelDb.Card<LittleGift>(),
        ModelDb.Card<AntiGravity>()
    ];

    // 初始遗物
    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<CrimsonScepter>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<AngelinaCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<AngelinaRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<AngelinaPotionPool>();
    
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath => "character_icon_angelina.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_angelina.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_angelina_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_angelina.png".CharacterUiPath();
}