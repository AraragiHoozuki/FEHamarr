using FEHamarr.SerializedData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FEHamarr.FEHArchive
{
    public interface IBlock
    {
        byte[] Binarize();
    } 
    public class FEHArc
    {
        public static readonly long HeadSize = 0x20;
        public static readonly byte[] XKeyId = {
            0x81, 0x00, 0x80, 0xA4, 0x5A, 0x16, 0x6F, 0x78,
            0x57, 0x81, 0x2D, 0xF7, 0xFC, 0x66, 0x0F, 0x27,
            0x75, 0x35, 0xB4, 0x34, 0x10, 0xEE, 0xA2, 0xDB,
            0xCC, 0xE3, 0x35, 0x99, 0x43, 0x48, 0xD2, 0xBB,
            0x93, 0xC1
        };
        public static readonly byte[] XKeyMsg = {
          0x6F, 0xB0, 0x8F, 0xD6, 0xEF, 0x6A, 0x5A, 0xEB, 0xC6, 0x76, 0xF6, 0xE5,
          0x56, 0x9D, 0xB8, 0x08, 0xE0, 0xBD, 0x93, 0xBA, 0x05, 0xCC, 0x26, 0x56,
          0x65, 0x1E, 0xF8, 0x2B, 0xF9, 0xA1, 0x7E, 0x41, 0x18, 0x21, 0xA4, 0x94,
          0x25, 0x08, 0xB8, 0x38, 0x2B, 0x98, 0x53, 0x76, 0xC6, 0x2E, 0x73, 0x5D,
          0x74, 0xCB, 0x02, 0xE8, 0x98, 0xAB, 0xD0, 0x36, 0xE5, 0x37
        };
        protected string path;
        protected Stream stream;
        protected FEHArcReader reader;
        protected byte[] xor_start;
        public string FilePath => path;
        public byte[] XStart => xor_start;

        public uint archive_size;
        public uint ptr_list_offset;
        public uint ptr_list_length;
        public uint ptr_taglist_length;
        public uint unknown1;
        public uint unknown2;
        public ulong magic;
        public FEHArc(string path)
        {
            this.path = path;
            if (Path.GetExtension(path).ToLower() == ".lz")
            {
                stream = new MemoryStream(Cryptor.ReadLZ(path, out xor_start));
            } else
            {
                throw new FormatException("Please Read an .lz file");
            }
            reader = new FEHArcReader(stream);
            ReadHead();
        }

        protected void ReadHead()
        {
            archive_size = reader.ReadUInt32();
            ptr_list_offset = reader.ReadUInt32();
            ptr_list_length = reader.ReadUInt32();
            ptr_taglist_length = reader.ReadUInt32();
            unknown1 = reader.ReadUInt32();
            unknown2 = reader.ReadUInt32();
            magic = reader.ReadUInt64();
        }
    }

    public class FEHArcSkill : FEHArc
    {
        public FEHArcSkill(string path) : base(path)
        {

        }

        public Dictionary<string, Skill> GetData()
        {
            var list = reader.ReadListPointer<SkillList>(SkillList.ListKey);
            reader.Close();
            return list.skills;
        }
    }

    public class FEHArcPerson : FEHArc
    {
        public FEHArcPerson(string path) : base(path)
        {

        }

        public Dictionary<string, Person> GetData()
        {
            var list = reader.ReadListPointer<PersonList>(PersonList.ListKey);
            reader.Close();
            return list.persons;
        }
    }

    public class FEHArcEnemy : FEHArc
    {
        public FEHArcEnemy(string path) : base(path)
        {
        }

        public Dictionary<string, Enemy> GetData()
        {
            var list = reader.ReadListPointer<EnemyList>(EnemyList.ListKey);
            reader.Close();
            return list.enemies;
        }
    }

    public class FEHArcSRPGMap : FEHArc
    {
        public FEHArcSRPGMap(string path) : base(path)
        {
        }

        public SRPGMap GetData()
        {
            SRPGMap map = new SRPGMap();
            map.unknown = reader.ReadUInt32();
            map.highest_score = reader.ReadX32(0xA9E250B1);
            long mark = reader.BaseStream.Position;
            reader.Skip(24);
            uint player_count = reader.ReadX32(0x9D63C79A);
            uint unit_count = reader.ReadX32(0xAC6710EE);
            reader.BaseStream.Seek(mark, SeekOrigin.Begin);
            map.field = reader.ReadPointer<Field>();
            PlayerPositions.count = (int)player_count;
            map.player_pos = reader.ReadPointer<PlayerPositions>();
            MapUnits.count = (int)unit_count;
            map.units = reader.ReadPointer<MapUnits>();
            reader.Close();
            return map;
        }
    }

    public class FEHArcMsg : FEHArc
    {
        public FEHArcMsg(string path) : base(path)
        {
        }

        public Dictionary<string, string> GetData()
        {
            Dictionary<string, string> msg_dict = new Dictionary<string, string>();
            ulong count = reader.ReadUInt64();
            for (ulong i = 0; i < count; i++)
            {
                msg_dict.Add(reader.ReadCryptedString(FEHArc.XKeyMsg), reader.ReadCryptedString(FEHArc.XKeyMsg));
            }
            reader.Close();
            return msg_dict;
        }
    }
}
