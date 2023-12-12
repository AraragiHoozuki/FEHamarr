using FEHamarr.FEHArchive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FEHamarr.SerializedData
{
    public class SRPGMap
    {
        public uint unknown;
        public uint highest_score;
        public ulong offset_field;
        public ulong offset_player_pos;
        public ulong offset_units;
        public uint player_count;
        public uint unit_count;
        public byte turns_to_win;
        public byte last_enemy_phase;
        public byte turns_to_defend;
        public byte[] padding;
        public Field field;
        public PlayerPositions player_pos;
        public MapUnits units;
        public string FieldId { get => field.id; set { field.id = value; } }

    }

    public enum TerrainType
    {
        Outdoor,
        Indoor,
        Desert,
        Forest,
        Mountain,
        River,
        Sea,
        Lava,
        Wall,
        OutdoorBreakable,
        OutdoorBreakable2,
        IndoorBreakable,
        IndoorBreakable2,
        DesertBreakable,
        DesertBreakable2,
        Bridge,
        OutdoorDefensive,
        ForestDefensive,
        IndoorDefensive,
        BridgeBreakable,
        BridgeBreakable2,
        Inaccessible,
        OutdoorTrench,
        IndoorTrench,
        OutdoorDefensiveTrench,
        IndoorDefensiveTrench,
        IndoorWater,
        PlayerFortress,
        EnemyFortress,
        PlayerCamp,
        EnemyCamp,
        OutdoorPlayerCamp,
        IndoorPlayerCamp,
        PlayerStructure,
        EnemyStructure
    }

    public class Field : IPointedStruct<Field>, IBlock
    {
        public string id;
        public uint width;
        public uint height;
        public byte base_terrain;
        public byte[] padding;
        public byte[][] terrain;//from left bottom

        public byte[] Binarize()
        {
            byte[] buffer;
            using (MemoryStream ms = new MemoryStream())
            using (FEHArcWriter w = new FEHArcWriter(ms))
            {
                w.WriteString(id, FEHArc.XKeyId);
                w.Write(width, 0x6b7cd75f);
                w.Write(height, 0x2baa12d5);
                w.Write(base_terrain, 0x41);
                w.Write(padding);
                foreach (byte[] line in terrain)
                {
                    foreach (byte tile in line)
                    {
                        w.Write(tile, 0xA1);
                    }
                }
                buffer = ms.ToArray();
            }
            return buffer;
        }

        public Field Read(FEHArcReader reader)
        {
            id = reader.ReadCryptedString(FEHArc.XKeyId);
            width = reader.ReadX32(0x6B7CD75F);
            height = reader.ReadX32(0x2BAA12D5);
            base_terrain = reader.ReadX8(0x41);
            padding = reader.ReadBytes(7);
            terrain = new byte[height][];
            for (int i = 0; i < height; i++)
            {
                terrain[i] = new byte[width];
                for (int j = 0; j < width; j++)
                {
                    terrain[i][j] = reader.ReadX8(0xA1);
                }
            }
            return this;
        }
    }

    public class Position
    {
        public ushort x;
        public ushort y;
    }

    public class  PlayerPositions : IPointedStruct<PlayerPositions>, IBlock
    {
        public static int count = 0;
        public Position[] list;

        public byte[] Binarize()
        {
            byte[] buffer;
            using (MemoryStream ms = new MemoryStream())
            using (FEHArcWriter w = new FEHArcWriter(ms))
            {
                foreach (Position p in list)
                {
                    w.Write(p.x, 0xb332);
                    w.Write(p.y, 0x28b2);
                    w.Write(new byte[4]);
                }
                buffer = ms.ToArray();
            }
            return buffer;
        }

        public PlayerPositions Read(FEHArcReader reader)
        {
            list = new Position[count];
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    list[i] = new Position()
                    {
                        x = reader.ReadX16(0xB332),
                        y = reader.ReadX16(0x28B2)
                    };
                    reader.Skip(4);
                }
            }
            return this;
        }
    }

    public class Unit
    {
        public string id_tag;
        public string[] skills;
        public string accessory;
        public Position pos;
        public byte rarity;
        public byte lv;
        public byte cd;
        public byte unknown = 100;
        public Stats stats;
        public byte start_turn;
        public byte movement_group;
        public byte movement_delay;
        public byte break_terrain;
        public byte tether;
        public byte true_lv;
        public byte is_enemy;
        public byte padding;
        public string spawn_check;
        public byte spawn_count;
        public byte spawn_turns;
        public byte spawn_target_remain;
        public byte spawn_target_kills;
        public byte[] paddings;

        public Unit Clone()
        {
            return (Unit)this.MemberwiseClone();
        }

        public static Unit Create(ushort x, ushort y)
        {
            return new Unit()
            {
                id_tag = "PID_無し",
                skills = new string[8],
                accessory = null,
                pos = new Position() { x = x, y = y },
                rarity = 5,
                lv = 40,
                cd = 255,
                stats = new Stats(),
                start_turn = 1,
                movement_group = 255,
                movement_delay = 255,
                break_terrain = 0,
                tether = 0,
                true_lv = 50,
                is_enemy = 0,
                padding = 0,
                spawn_check = null,
                spawn_count = 255,
                spawn_turns = 255,
                spawn_target_remain = 255,
                spawn_target_kills = 255,
                paddings = new byte[4]
            };
        }
    }

    public class MapUnits : IPointedStruct<MapUnits>, IBlock
    {
        public static int count = 0;
        public List<Unit> list;

        public byte[] Binarize()
        {
            byte[] buffer;
            using (MemoryStream ms = new MemoryStream())
            using (FEHArcWriter w = new FEHArcWriter(ms))
            {
                foreach (Unit u in list)
                {
                    w.WriteString(u.id_tag, FEHArc.XKeyId);
                    foreach (string sk in u.skills)
                        w.WriteString(sk, FEHArc.XKeyId);
                    w.WriteString(u.accessory, FEHArc.XKeyId);
                    w.Write(u.pos.x, 0xb332);
                    w.Write(u.pos.y, 0x28b2);
                    w.Write(u.rarity, 0x61);
                    w.Write(u.lv, 0x2a);
                    w.Write(u.cd, 0x1e);
                    w.Write(u.unknown);
                    w.WriteStats(u.stats);
                    w.Write(u.start_turn, 0xcf);
                    w.Write(u.movement_group, 0xf4);
                    w.Write(u.movement_delay, 0x95);
                    w.Write(u.break_terrain, 0x71);
                    w.Write(u.tether, 0xb8);
                    w.Write(u.true_lv, 0x85);
                    w.Write(u.is_enemy, 0xd0);
                    w.Write(u.padding);
                    w.WriteString(u.spawn_check, FEHArc.XKeyId);
                    w.Write(u.spawn_count, 0x0a);
                    w.Write(u.spawn_turns, 0x0a);
                    w.Write(u.spawn_target_remain, 0x2d);
                    w.Write(u.spawn_target_kills, 0x58);
                    w.Write(u.paddings);
                }
                buffer = ms.ToArray();
            }
            return buffer;
        }

        public MapUnits Read(FEHArcReader reader)
        {
            list = new List<Unit>();
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    list.Add(new Unit()
                    {
                        id_tag = reader.ReadCryptedString(FEHArc.XKeyId),
                        skills = new string[8]
                        {
                            reader.ReadCryptedString(FEHArc.XKeyId),
                            reader.ReadCryptedString(FEHArc.XKeyId),
                            reader.ReadCryptedString(FEHArc.XKeyId),
                            reader.ReadCryptedString(FEHArc.XKeyId),
                            reader.ReadCryptedString(FEHArc.XKeyId),
                            reader.ReadCryptedString(FEHArc.XKeyId),
                            reader.ReadCryptedString(FEHArc.XKeyId),
                            reader.ReadCryptedString(FEHArc.XKeyId),
                        },
                        accessory = reader.ReadCryptedString(FEHArc.XKeyId),
                        pos = new Position()
                        {
                            x = reader.ReadX16(0xB332),
                            y = reader.ReadX16(0x28B2)
                        },
                        rarity = reader.ReadX8(0x61),
                        lv = reader.ReadX8(0x2A),
                        cd = reader.ReadX8(0x1E),
                        unknown = reader.ReadByte(),
                        stats = reader.ReadData<Stats>(),
                        start_turn = reader.ReadX8(0xCF),
                        movement_group = reader.ReadX8(0xF4),
                        movement_delay = reader.ReadX8(0x95),
                        break_terrain = reader.ReadX8(0x71),
                        tether = reader.ReadX8(0xB8),
                        true_lv = reader.ReadX8(0x85),
                        is_enemy = reader.ReadX8(0xD0),
                        padding = reader.ReadByte(),
                        spawn_check = reader.ReadCryptedString(FEHArc.XKeyId),
                        spawn_count = reader.ReadX8(0x0A),
                        spawn_turns = reader.ReadX8(0x0A),
                        spawn_target_remain = reader.ReadX8(0x2D),
                        spawn_target_kills = reader.ReadX8(0x5B),
                        paddings = reader.ReadBytes(4)
                    });
                }
            }
            return this;
        }
    }
}
