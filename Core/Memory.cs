using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Core
{

    public class VadTable {
        public class Vad {
            public readonly long Length;
            private readonly SortedList<long, byte> _entries;

            public IReadOnlyDictionary<long, byte> Entries { get; }

            public Vad(long length) {
                Length = length;
                _entries = new SortedList<long, byte>((int)length);
                Entries = _entries;
            }

            public void SetOwner(long pos, byte owner) {
                var idx = _entries.IndexOfKey(pos);
                if (idx == -1) {
                    _entries.Add(pos, owner);
                } else {
                    _entries[idx] = owner;
                }
            }

            public Dictionary<byte, long> GetOwnershipStats() 
                => _entries.GroupBy(o => o.Value).ToDictionary(o => o.Key, o => o.LongCount());

            public override string ToString()
                => String.Join(", ", GetOwnershipStats().Select(o=> $"CPU {o.Key}: {o.Value} address(es)"));
        }
        public Vad[] Vads { get; }
        public readonly long VadSize;
        public VadTable(long memsize, long vadsize) {
            if (memsize % vadsize != 0) {
                throw new ArgumentOutOfRangeException("Memory size must be a multiple of vad size");
            }
            VadSize = vadsize;
            Vads = new Vad[(int)(memsize / vadsize)];
            Console.WriteLine($"Memory - Creating {Vads.Length} VADs...");
            for (int i = 0; i < Vads.Length; i++) {
                Vads[i] = new Vad(vadsize);
            }
        }

        public void SetOwner(long pos, byte owner) {
            var vad = (int)(pos / VadSize);
            Vads[vad].SetOwner(pos, owner);
        }

        public override string ToString() {
            var builder = new StringBuilder();

            for (var i = 0; i < Vads.Length; i++) {
                if (Vads[i].Entries.Count == 0)
                    continue;
                builder.AppendLine();
                var stats = Vads[i].GetOwnershipStats();
                var stat_str = String.Join(", ", stats.Select(o => $"{(double)o.Value / VadSize:P}: {o.Key}"));
                builder.AppendFormat("\tVAD {0} (0x{1:X}-0x{2:X}, {3}): {4}", i, i * VadSize, (i * VadSize) + (VadSize - 1), stat_str, Vads[i]);
            }

            return builder.ToString();
        }
    }

    public class Memory {
        public Stream Stream { get; }
        public long Size { get; }
        public byte[] RawMemory { get; }
        public VadTable MemoryOwnership { get; }

        public Memory(long size, long vadsize) {
            RawMemory = new byte[size];
            MemoryOwnership = new VadTable(size, vadsize);
            Stream = (Stream)new CircularStream(new MemoryStream(RawMemory));
            Size = size;
        }

        public Memory(byte[] raw, long vadsize) {
            RawMemory = raw;
            MemoryOwnership = new VadTable(raw.Length, vadsize);
            Stream = new MemoryStream(RawMemory);
            Size = raw.Length;
        }

        public void WriteRaw(long position, UInt32 value, byte CpuID) {
            var bytes = BitConverter.GetBytes(value);
            for (var i = 0; i < bytes.Length; i++) {
                position %= Size;
                RawMemory[position] = bytes[i];
                MemoryOwnership.SetOwner(position++, CpuID);
            }
        }

        public UInt32 ReadRaw(long position) {
            var bytes = new byte[4];
            for (var i = 0; i < bytes.Length; i++) {
                position %= Size;
                bytes[i] = RawMemory[position++];
            }
            return BitConverter.ToUInt32(bytes, 0);
        }
    }
}
