using Avalonia.Controls.Shapes;
using FEHamarr.FEHArchive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEHamarr.SerializedData
{
    public interface IPointedStruct<T>
    {
        T Read(FEHArcReader reader);
    }

    public interface IPointedList<T>
    {
        ulong count { get; set; }
        T Read(FEHArcReader reader, ulong count);
    }
    public enum WeaponType
    {
        Sword, Lance, Axe, RedBow, BlueBow, GreenBow, ColorlessBow, RedDagger, BlueDagger,
        GreenDagger, ColorlessDagger, RedTome, BlueTome, GreenTome, ColorlessTome, Staff, RedBreath, BlueBreath, GreenBreath,
        ColorlessBreath, RedBeast, BlueBeast, GreenBeast, ColorlessBeast
    };
    public enum Element { None, Fire, Thunder, Wind, Light, Dark };
    public enum MoveType { Infantry, Armored, Cavalry, Flying };
    
    public class Stats : IPointedStruct<Stats>
    {
        public ushort hp;
        public ushort atk;
        public ushort spd;
        public ushort def;
        public ushort res;

        public int Total => hp + atk + spd + def + res;

        public int this[int index]
        {
            get
            {
                int value = index switch
                {
                    0 => hp,
                    1 => atk,
                    2 => spd,
                    3 => def,
                    4 => res,
                    _ => 0,
                };
                return value;
            }
        }

        public Stats Read(FEHArcReader reader)
        {
            hp = reader.ReadX16(0xD632);
            atk = reader.ReadX16(0x14A0);
            spd = reader.ReadX16(0xA55E);
            def = reader.ReadX16(0x8566);
            res = reader.ReadX16(0xAEE5);
            reader.Skip(6);
            return this;
        }
    }
    public class Person
    {
        public string id;
        public string roman;
        public string face;
        public string face2;
        public byte[] legendary;//16 bytes Legendary info
        public ulong timestamp;
        public uint id_num;
        public uint version_num = 65535;
        public uint sort_value;
        public uint origins;
        public WeaponType weapon_type;
        public Element tome_class;
        public MoveType move_type;
        public byte series;
        public byte regular;
        public byte permanent;
        public byte base_vector;
        public byte is_refresher;
        public byte dragonflowers;
        public byte[] unknown; //7bytes offset
        public Stats stats;
        public Stats grow;
        public string[][] skills;

        public int Stat(int index, int hone = 0, int level = 40)
        {
            int value = grow[index] + 5 * hone;
            value = value * 114 / 100;
            value = value * (level - 1) / 100;
            value = value + stats[index] + 1 + hone;
            return value;
        }

        public int[] CalcStats(int level, int merge, int honeIndex, int flawIndex)
        {
            int[] temp = new int[] { stats.hp, stats.atk, stats.spd, stats.def, stats.res };
            if (honeIndex > 0) temp[honeIndex] += 1;
            if (flawIndex > 0) temp[flawIndex] -= 1;
            var order = temp.Select((n, i) => new { Value = n, Index = i }).OrderByDescending(x => x.Value);
            int[] res = new int[5];

            for (int mt = 0; mt < merge; mt++)
            {
                switch (mt) {
                    case 0:
                        res[order.Skip(0).First().Index] += 1;
                        res[order.Skip(1).First().Index] += 1;
                        if (flawIndex < 0)
                        {
                            res[order.Skip(0).First().Index] += 1;
                            res[order.Skip(1).First().Index] += 1;
                            res[order.Skip(2).First().Index] += 1;
                        }
                        break;
                    case 1:
                    case 6:
                        res[order.Skip(2).First().Index] += 1;
                        res[order.Skip(3).First().Index] += 1;
                        break;
                    case 2:
                    case 7:
                        res[order.Skip(0).First().Index] += 1;
                        res[order.Skip(4).First().Index] += 1;
                        break;
                    case 3:
                    case 8:
                        res[order.Skip(1).First().Index] += 1;
                        res[order.Skip(2).First().Index] += 1;
                        break;
                    case 4:
                    case 9:
                        res[order.Skip(3).First().Index] += 1;
                        res[order.Skip(4).First().Index] += 1;
                        break;
                    case 5:
                        res[order.Skip(0).First().Index] += 1;
                        res[order.Skip(1).First().Index] += 1;
                        break;
                    default:
                        break;

                }
                
            }

            if (merge > 0) flawIndex = -1;
            for(int i = 0; i < 5; i++)
            {
                res[i] += Stat(i, honeIndex == i ? 1 : (flawIndex == i ? -1 : 0), level);
            }
            return res;
        }
    }

    public class Enemy : Person
    {
        public string top_weapon;
        public string assist1;
        public string assist2;
        public string unknown_string;
        public byte _unknown;
        public byte random_allowed;
        public byte is_boss;
        public byte is_refresher;
        public byte is_enemy;
        public byte is_npc;
    }

    public class PersonList : IPointedList<PersonList>
    {
        public ulong count { get; set; }
        public Dictionary<string, Person> persons;
        public static ulong ListKey = 0xDE51AB793C3AB9E1;

        public PersonList Read(FEHArcReader reader, ulong count)
        {
            this.count = count;
            persons = new Dictionary<string, Person>();
            for (ulong i = 0; i < count; i++)
            {
                Person item = new Person
                {
                    id = reader.ReadCryptedString(FEHArc.XKeyId),
                    roman = reader.ReadCryptedString(FEHArc.XKeyId),
                    face = reader.ReadCryptedString(FEHArc.XKeyId),
                    face2 = reader.ReadCryptedString(FEHArc.XKeyId),
                    legendary = reader.ReadBytes(16),
                    timestamp = reader.ReadX64(0xBDC1E742E9B6489B),
                    id_num = reader.ReadX32(0x5F6E4E18),
                    version_num = reader.ReadX32(0x2E193A3C),
                    sort_value = reader.ReadX32(0x2A80349B),
                    origins = reader.ReadX32(0xE664B808),
                    weapon_type = (WeaponType)reader.ReadX8(0x06),
                    tome_class = (Element)reader.ReadX8(0x35),
                    move_type = (MoveType)reader.ReadX8(0x2A),
                    series = reader.ReadX8(0x43),
                    regular = reader.ReadX8(0xA1),
                    permanent = reader.ReadX8(0xC7),
                    base_vector = reader.ReadX8(0x3D),
                    is_refresher = reader.ReadX8(0xFF),
                    dragonflowers = reader.ReadX8(0xE4),
                    unknown = reader.ReadBytes(7),
                    stats = reader.ReadData<Stats>(),
                    grow = reader.ReadData<Stats>(),
                    skills = new string[5][],
                };
                for (int j = 0; j < 5; j++)
                {
                    item.skills[j] = new string[15];
                    for (int k = 0; k < 15; k++)
                    {
                        item.skills[j][k] = reader.ReadCryptedString(FEHArc.XKeyId);
                    }
                }
                persons.Add(item.id, item);
            }
            return this;
        }
    }

    public class EnemyList : IPointedList<EnemyList>
    {
        public ulong count { get; set; }
        public Dictionary<string, Enemy> enemies;
        public static ulong ListKey = 0x62CA95119CC5345C;

        public EnemyList Read(FEHArcReader reader, ulong count)
        {
            this.count = count;
            enemies = new Dictionary<string, Enemy>();
            for (ulong i = 0; i < count; i++)
            {
                Enemy item = new Enemy
                {
                    id = reader.ReadCryptedString(FEHArc.XKeyId),
                    roman = reader.ReadCryptedString(FEHArc.XKeyId),
                    face = reader.ReadCryptedString(FEHArc.XKeyId),
                    face2 = reader.ReadCryptedString(FEHArc.XKeyId),
                    top_weapon = reader.ReadCryptedString(FEHArc.XKeyId),
                    assist1 = reader.ReadCryptedString(FEHArc.XKeyId),
                    assist2 = reader.ReadCryptedString(FEHArc.XKeyId),
                    unknown_string = reader.ReadCryptedString(FEHArc.XKeyId),
                    timestamp = reader.ReadX64(0xBDC1E742E9B6489B),
                    id_num = reader.ReadX32(0x5F6E4E18),
                    unknown = reader.ReadBytes(4),
                    weapon_type = (WeaponType)reader.ReadX8(0xE4),
                    tome_class = (Element)reader.ReadX8(0x81),
                    move_type = (MoveType)reader.ReadX8(0x0D),
                    random_allowed = reader.ReadX8(0xC4),
                    is_boss = reader.ReadX8(0x6A),
                    is_refresher = reader.ReadX8(0x2A),
                    is_enemy = reader.ReadX8(0x13),
                    is_npc = reader.ReadX8(0),
                    stats = reader.ReadData<Stats>(),
                    grow = reader.ReadData<Stats>(),
                    version_num = 0,
                };
                enemies.Add(item.id, item);
            }
            return this;
        }
    }

}
