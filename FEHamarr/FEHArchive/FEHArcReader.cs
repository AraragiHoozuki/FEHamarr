using System.Collections.Generic;
using System.IO;
using FEHamarr.SerializedData;
using System.Text;

namespace FEHamarr.FEHArchive
{
    public class FEHArcReader : BinaryReader
    {
        public FEHArcReader(Stream input) : base(input)
        {
        }

        public byte ReadX8(byte key)
        {
            return (byte)(ReadByte() ^ key);
        }
        public ushort ReadX16(ushort key)
        {
            return (ushort)(ReadUInt16() ^ key);
        }
        public uint ReadX32(uint key)
        {
            return ReadUInt32() ^ key;
        }
        public ulong ReadX64(ulong key)
        {
            return ReadUInt64() ^ key;
        }

        public void Skip(int byte_num) {
            BaseStream.Seek(byte_num, SeekOrigin.Current);
        }

        protected byte[] ReadTilZero()
        {
            List<byte> list = new List<byte>();
            byte reading = ReadByte();
            while (reading != 0)
            {
                list.Add(reading);
                reading = ReadByte();
            }
            return list.ToArray();
        }

        public T? ReadPointer<T>() where T : IPointedStruct<T>, new()
        {
            ulong offset = ReadUInt64();
            if (offset == 0)
            {
                return default;
            }
            else {
                long pos = BaseStream.Position;
                BaseStream.Seek(FEHArc.HeadSize + (long)offset, SeekOrigin.Begin);
                T data =  ReadData<T>();
                BaseStream.Seek(pos, SeekOrigin.Begin);
                return data;
            }
        }

        public T? ReadListPointer<T>(ulong list_key) where T : IPointedList<T>, new()
        {
            ulong offset = ReadUInt64();
            ulong count = ReadX64(list_key);
            if (offset == 0)
            {
                return default;
            }
            else
            {
                long pos = BaseStream.Position;
                BaseStream.Seek(FEHArc.HeadSize + (long)offset, SeekOrigin.Begin);
                T data = ReadList<T>(count);
                BaseStream.Seek(pos, SeekOrigin.Begin);
                return data;
            }
        }

        public T ReadData<T>() where T : IPointedStruct<T>, new()
        {
            T data = new T();
            data.Read(this); 
            return data;
        }

        public T ReadList<T>(ulong count) where T : IPointedList<T>, new()
        {
            T data = new T();
            data.Read(this, count);
            return data;
        }
        public string? ReadCryptedString(byte[]? key = null)
        {
            ulong str_addr = ReadUInt64();
            long pos = BaseStream.Position;
            byte[] enc;
            if (str_addr != 0)
            {
                BaseStream.Seek(FEHArc.HeadSize + (long)str_addr, SeekOrigin.Begin);
                enc = ReadTilZero();
                BaseStream.Seek(pos, SeekOrigin.Begin);
                if (key == null)
                {
                    return Encoding.UTF8.GetString(enc);
                }
                else
                {
                    for (int i = 0; i < enc.Length; i++)
                    {
                        if (enc[i] != key[i % key.Length]) enc[i] ^= key[i % key.Length];
                    }
                    return Encoding.UTF8.GetString(enc);
                }
            }
            return null;
        }
    }
}
