using FEHamarr.SerializedData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace FEHamarr.FEHArchive
{
    public class FEHArcWriter : BinaryWriter
    {
        public Dictionary<long, byte[]> pointers = new Dictionary<long, byte[]>();
        long pointer_list_offset;
        public FEHArcWriter(Stream output) : base(output)
        {
        }
        public void Write(byte value, byte key)
        {
            Write((byte)(value ^ key));
        }
        public void Write(ushort value, ushort key)
        {
            Write((ushort)(value ^ key));
        }
        public void Write(short value, ushort key)
        {
            Write((short)(value ^ key));
        }
        public void Write(uint value, uint key)
        {
            Write((uint)(value ^ key));
        }
        public void Write(ulong value, ulong key)
        {
            Write(value ^ key);
        }

        public void WriteStats(Stats st)
        {
            Write(st.hp, 0xd632);
            Write(st.atk, 0x14a0);
            Write(st.spd, 0xa55e);
            Write(st.def, 0x8566);
            Write(st.res, 0xaee5);
            Write(new byte[] { 0x57, 0x64, 0x1a, 0x29, 0x59, 0x05 });
        }

        public void WriteString(string text, byte[]? key)
        {
            if (text == null)
            {
                EmptyPointer();
            } else if (key == null)
            {
                Write(text);
            } else
            {
                byte[] codes = Encoding.UTF8.GetBytes(text);
                byte[] xored = new byte[(codes.Length + 8) / 8 * 8];
                for (int i = 0; i < codes.Length; i++)
                {
                    if (codes[i] != key[i % key.Length])
                    {
                        xored[i] = (byte)(codes[i] ^ key[i % key.Length]);
                    }
                    else
                    {
                        xored[i] = codes[i];
                    }
                }
                AddPtr(xored);
            }
            
            
        }

        public void EmptyPointer()
        {
            Write((long)0);
        }

        public long AddPtr(byte[] buffer)
        {
            long pos = BaseStream.Position;
            pointers.Add(pos, buffer);
            Write((long)0);
            return pos;
        }

        public void AddPtr(IBlock block) {
            if (block == null)
            {
                EmptyPointer();
            } else
            {
                AddPtr(block.Binarize());
            }
        }

        public void UpdatePointer(long block_offset, long ptr_offset)
        {
            long curr = BaseStream.Position;
            BaseStream.Seek(ptr_offset, SeekOrigin.Begin);
            Write(block_offset);
            BaseStream.Seek(curr, SeekOrigin.Begin);
        }

        public void WritePointerBlocks()
        {
            
            foreach (KeyValuePair<long, byte[]> p in pointers)
            {
                if (p.Value == null || p.Value.Length < 1) continue;
                UpdatePointer(BaseStream.Position - FEHArc.HeadSize, p.Key);
                Write(p.Value);
            }
        }

        public void WritePointerOffsets()
        {
            pointer_list_offset = BaseStream.Position;
            foreach (KeyValuePair<long, byte[]> p in pointers)
            {
                Write(p.Key - FEHArc.HeadSize);
            }
        }

        public void WriteStart()
        {
            Write(new byte[FEHArc.HeadSize]);
        }

        public void WriteEnd(uint unknown1, uint unknown2, ulong magic) {
            int size = (int)BaseStream.Position;
            BaseStream.Seek(0, SeekOrigin.Begin);
            Write(size);
            Write((uint)(pointer_list_offset - FEHArc.HeadSize));
            Write(pointers.Count);
            Write((uint)0);
            Write(unknown1);
            Write(unknown2);
            Write(magic);
        }
    }


    public class SRPGMapWriter : FEHArcWriter
    {
        public SRPGMapWriter(Stream output) : base(output)
        {
        }

        public void WriteAll(FEHArcSRPGMap _arc, SRPGMap _map)
        {
            WriteStart();
            Write(_map.unknown);
            Write(_map.highest_score, 0xa9e250b1);
            long field_ptr_pos = AddPtr(new byte[0]); // field offset
            long player_ptr_pos = AddPtr(new byte[0]); // player pos offset
            long units_ptr_pos = AddPtr(new byte[0]); // units offset
            //AddPtr(_map.field);
            //AddPtr(_map.player_pos);
            //AddPtr(_map.units);
            Write((ushort)_map.player_pos.list.Count(), 0x9d63c79a);
            Write((ushort)_map.units.list.Count(), 0xac6710ee);
            Write(_map.turns_to_win, 0xfd);
            Write(_map.last_enemy_phase, 0xc7);
            Write(_map.turns_to_defend, 0xec);
            Write(new byte[5]);
            UpdatePointer(BaseStream.Position - FEHArc.HeadSize, field_ptr_pos);
            WriteField(_map.field);
            UpdatePointer(BaseStream.Position - FEHArc.HeadSize, player_ptr_pos);
            foreach (Position p in _map.player_pos.list)
            {
                Write(p.x, 0xb332);
                Write(p.y, 0x28b2);
                Write(new byte[4]);
            }
            UpdatePointer(BaseStream.Position - FEHArc.HeadSize, units_ptr_pos);
            foreach (Unit u in _map.units.list)
            {
                WriteUnit(u);
            }
            WritePointerBlocks();
            WritePointerOffsets();
            WriteEnd(_arc.unknown1, _arc.unknown2, _arc.magic);
        }

        public void WriteUnit(Unit u)
        {
            WriteString(u.id_tag, FEHArc.XKeyId);
            foreach (string sk in u.skills)
                WriteString(sk, FEHArc.XKeyId);
            WriteString(u.accessory, FEHArc.XKeyId);
            Write(u.pos.x, 0xb332);
            Write(u.pos.y, 0x28b2);
            Write(u.rarity, 0x61);
            Write(u.lv, 0x2a);
            Write(u.cd, 0x1e);
            Write(u.unknown);
            WriteStats(u.stats);
            Write(u.start_turn, 0xcf);
            Write(u.movement_group, 0xf4);
            Write(u.movement_delay, 0x95);
            Write(u.break_terrain, 0x71);
            Write(u.tether, 0xb8);
            Write(u.true_lv, 0x85);
            Write(u.is_enemy, 0xd0);
            Write(u.padding);
            WriteString(u.spawn_check, FEHArc.XKeyId);
            Write(u.spawn_count, 0x0a);
            Write(u.spawn_turns, 0x0a);
            Write(u.spawn_target_remain, 0x2d);
            Write(u.spawn_target_kills, 0x58);
            Write(u.paddings);
        }

        public void WriteField(Field field)
        {
            WriteString(field.id, FEHArc.XKeyId);
            Write(field.width, 0x6b7cd75f);
            Write(field.height, 0x2baa12d5);
            Write(field.base_terrain, 0x41);
            Write(field.padding); // 7 bytes padding
            foreach (byte[] line in field.terrain)
            {
                foreach (byte tile in line)
                {
                    Write(tile, 0xA1);
                }
            }
        }
    }

}
