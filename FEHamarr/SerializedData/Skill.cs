using FEHamarr.FEHArchive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEHamarr.SerializedData
{
    public enum SkillCategory
    {
        Weapon,
        Assist,
        Special,
        A,
        B,
        C,
        S,
        Refine,
        Transform
    }
    public class Skill
    {
        public string id;
        public string refine_base;
        public string name;
        public string description;
        public string refine_id;
        public string beast_effect_id;
        public string[] requirements; //2-length
        public string next_skill;
        public string[] sprites;// 4-length
        public Stats stats;
        public Stats class_params;
        public Stats combat_buffs;
        public Stats skill_params;
        public Stats skill_params2;
        public Stats refine_stats;
        public uint id_num;
        public uint sort_value;
        public uint icon;
        public uint wep_equip;
        public uint mov_equip;
        public uint sp_cost;
        public SkillCategory category;
        public Element tome_class;
        public byte is_exclusive;
        public byte enemy_only;
        public byte range;
        public byte might;
        public byte cooldown;
        public byte assist_cd;
        public byte healing;
        public byte skill_range;
        public ushort score;
        public byte promotion_tier;
        public byte promotion_rarity;
        public byte is_refined;
        public byte refine_sort_id;
        public uint tokkou_wep;
        public uint tokkou_mov;
        public uint shield_wep;
        public uint shield_mov;
        public uint weak_wep;
        public uint weak_mov;
        public uint unknown1;
        public uint unknown2;
        public uint adaptive_wep;
        public uint adaptive_mov;
        public uint timing;
        public uint ability;
        public uint limit1;
        public ushort[] limit1_params;//2-length
        public uint limit2;
        public ushort[] limit2_params;//2-length
        public uint target_wep;
        public uint target_mov;
        public string passive_next;
        public ulong timestamp;
        public byte random_allowed;
        public byte min_lv;
        public byte max_lv;
        public byte tt_inherit_base;
        public byte random_mode;
        public byte[] padding1; //3-length
        public uint limit3;
        public ushort[] limit3_params;//2-length
        public byte range_shape;
        public byte target_either;
        public byte distant_counter;
        public byte canto;
        public byte pathfinder;
        public byte[] padding2; //3-length

        public string Name => DataManager.GetMessage(name);

        public string Description
        {
            get
            {
                string s = "";
                if (this.category == SkillCategory.Weapon)
                {
                    s += $"ATK: {might}\n";
                }
                if (this.category == SkillCategory.Special)
                {
                    s += $"CD: {cooldown}\n";
                }
                s += DataManager.GetMessage(description) + "\n";
                if (refine_id != null)
                {
                    s += DataManager.GetMessage(DataManager.GetSkill(refine_id).description);
                }
                return s;
            }
        }
    }

    public class SkillList : IPointedList<SkillList>
    {
        public ulong count { get; set; }
        public Dictionary<string, Skill> skills;
        public static ulong ListKey = 0x7FECC7074ADEE9AD;

        public SkillList Read(FEHArcReader reader, ulong count)
        {
            this.count = count;
            skills = new Dictionary<string, Skill>();
            for (ulong i = 0; i < count; i++)
            {
                Skill item = new Skill
                {
                    id = reader.ReadCryptedString(FEHArc.XKeyId),
                    refine_base = reader.ReadCryptedString(FEHArc.XKeyId),
                    name = reader.ReadCryptedString(FEHArc.XKeyId),
                    description = reader.ReadCryptedString(FEHArc.XKeyId),
                    refine_id = reader.ReadCryptedString(FEHArc.XKeyId),
                    beast_effect_id = reader.ReadCryptedString(FEHArc.XKeyId),
                    requirements = new string[2] { reader.ReadCryptedString(FEHArc.XKeyId), reader.ReadCryptedString(FEHArc.XKeyId) },
                    next_skill = reader.ReadCryptedString(FEHArc.XKeyId),
                    sprites = new string[4] { reader.ReadCryptedString(null), reader.ReadCryptedString(null), reader.ReadCryptedString(null), reader.ReadCryptedString(null) },
                    stats = reader.ReadData<Stats>(),
                    class_params = reader.ReadData<Stats>(),
                    combat_buffs = reader.ReadData<Stats>(),
                    skill_params = reader.ReadData<Stats>(),
                    skill_params2 = reader.ReadData<Stats>(),
                    refine_stats = reader.ReadData<Stats>(),
                    id_num = reader.ReadX32(0xC6A53A23),
                    sort_value = reader.ReadX32(0x8DDBF8AC),
                    icon = reader.ReadX32(0xC6DF2173),
                    wep_equip = reader.ReadX32(0x35B99828),
                    mov_equip = reader.ReadX32(0xAB2818EB),
                    sp_cost = reader.ReadX32(0xC031F669),
                    category = (SkillCategory)reader.ReadX8(0xBC),
                    tome_class = (Element)reader.ReadX8(0x35),
                    is_exclusive = reader.ReadX8(0xCC),
                    enemy_only = reader.ReadX8(0x4F),
                    range = reader.ReadX8(0x56),
                    might = reader.ReadX8(0xD2),
                    cooldown = reader.ReadX8(0x56),
                    assist_cd = reader.ReadX8(0xF2),
                    healing = reader.ReadX8(0x95),
                    skill_range = reader.ReadX8(0x09),
                    score = reader.ReadX16(0xA232),
                    promotion_tier = reader.ReadX8(0xE0),
                    promotion_rarity = reader.ReadX8(0x75),
                    is_refined = reader.ReadX8(0x02),
                    refine_sort_id = reader.ReadX8(0xFC),
                    tokkou_wep = reader.ReadX32(0x23BE3D43),
                    tokkou_mov = reader.ReadX32(0x823FDAEB),
                    shield_wep = reader.ReadX32(0xAABAB743),
                    shield_mov = reader.ReadX32(0x0EBEF25B),
                    weak_wep = reader.ReadX32(0x005A02AF),
                    weak_mov = reader.ReadX32(0xB269B819),
                    unknown1 = reader.ReadUInt32(),
                    unknown2 = reader.ReadUInt32(),
                    adaptive_wep = reader.ReadX32(0x494E2629),
                    adaptive_mov = reader.ReadX32(0xEE6CEF2E),
                    timing = reader.ReadX32(0x9C776648),
                    ability = reader.ReadX32(0x72B07325),
                    limit1 = reader.ReadX32(0x0EBDB832),
                    limit1_params = new ushort[2] { reader.ReadX16(0xA590), reader.ReadX16(0xA590) },
                    limit2 = reader.ReadX32(0x0EBDB832),
                    limit2_params = new ushort[2] { reader.ReadX16(0xA590), reader.ReadX16(0xA590) },
                    target_wep = reader.ReadX32(0x409FC9D7),
                    target_mov = reader.ReadX32(0x6C64D122),
                    passive_next = reader.ReadCryptedString(FEHArc.XKeyId),
                    timestamp = reader.ReadX64(0xED3F39F93BFE9F51),
                    random_allowed = reader.ReadX8(0x10),
                    min_lv = reader.ReadX8(0x90),
                    max_lv = reader.ReadX8(0x24),
                    tt_inherit_base = reader.ReadX8(0x19),
                    random_mode = reader.ReadX8(0xBE),
                    padding1 = reader.ReadBytes(3),
                    limit3 = reader.ReadX32(0x0EBDB832),
                    limit3_params = new ushort[2] { reader.ReadX16(0xA590), reader.ReadX16(0xA590) },
                    range_shape = reader.ReadX8(0x5C),
                    target_either = reader.ReadX8(0xA7),
                    distant_counter = reader.ReadX8(0xDB),
                    canto = reader.ReadX8(0x41),
                    pathfinder = reader.ReadX8(0xBE),
                    padding2 = reader.ReadBytes(3)
                };
                skills.Add(item.id, item);
            }
            return this;
        }
    }
}
