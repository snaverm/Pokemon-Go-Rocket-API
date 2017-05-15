using System;
using System.Collections.Generic;
using System.Linq;

class NiaHash
{

    /* IOS 1.15.0 */
    static ulong[] magic_table = {
      0x2dd7caaefcf073eb, 0xa9209937349cfe9c,
      0xb84bfc934b0e60ef, 0xff709c157b26e477,
      0x3936fd8735455112, 0xca141bf22338d331,
      0xdd40e749cb64fd02, 0x5e268f564b0deb26,
      0x658239596bdea9ec, 0x31cedf33ac38c624,
      0x12f56816481b0cfd, 0x94e9de155f40f095,
      0x5089c907844c6325, 0xdf887e97d73c50e3,
      0xae8870787ce3c11d, 0xa6767d18c58d2117,
    };

    static UInt128 ROUND_MAGIC = new UInt128(0xe3f0d44988bcdfab, 0x081570afdd535ec3);
    static ulong FINAL_MAGIC0 = 0xce7c4801d683e824;
    static ulong FINAL_MAGIC1 = 0x6823775b1daad522;

    static ulong read_int64(byte[] p, int offset) { return BitConverter.ToUInt64(p, offset); }

    public static ulong compute_hash64(byte[] input, UInt32 seed)
    {
        byte[] inputWithSeed = new byte[input.Length + 4];
        byte[] seedArr = BitConverter.GetBytes(seed).Reverse().ToArray();
        Array.Copy(seedArr, inputWithSeed, 4);
        Array.Copy(input, 0, inputWithSeed, 4, input.Length);
        return compute_hash(inputWithSeed);
    }
    public static uint compute_hash32(byte[] input, UInt32 seed)
    {
        byte[] inputWithSeed = new byte[input.Length + 4];
        byte[] seedArr = BitConverter.GetBytes(seed).Reverse().ToArray();
        Array.Copy(seedArr, inputWithSeed, 4);
        Array.Copy(input, 0, inputWithSeed, 4, input.Length);
        ulong hash = compute_hash(inputWithSeed);
        return ((uint)hash)^ ((uint)(hash>>32)) ;
    }
    public static ulong compute_hash64(byte[] input, UInt64 seed)
    {
        byte[] inputWithSeed = new byte[input.Length + 8];
        byte[] seedArr = BitConverter.GetBytes(seed).Reverse().ToArray();
        Array.Copy(seedArr, inputWithSeed, 8);
        Array.Copy(input, 0, inputWithSeed, 8, input.Length);
        return compute_hash(inputWithSeed);
    }

    public static ulong compute_hash(byte[] input)
    {
        uint num_chunks = (uint)input.Length / 128;

        // copy tail, pad with zeroes
        byte[] tail = new byte[128];
        uint tail_size = (uint)input.Length % 128;
        Array.Copy(input, (int)(input.Length - tail_size), tail, 0, (int)tail_size);

        UInt128 hash;

        if (num_chunks != 0) hash = hash_chunk(input, 128, 0);
        else hash = hash_chunk(tail, tail_size, 0);

        hash += ROUND_MAGIC;

        int offset = 0;

        if (num_chunks != 0)
        {
            while (--num_chunks > 0)
            {
                offset += 128;
                hash = hash_muladd(hash, ROUND_MAGIC, hash_chunk(input, 128, offset));
            }

            if (tail_size > 0)
            {
                hash = hash_muladd(hash, ROUND_MAGIC, hash_chunk(tail, tail_size, 0));
            }
        }

        hash += new UInt128(tail_size * 8, 0);

        if (hash > new UInt128(0x7fffffffffffffff, 0xffffffffffffffff)) hash++;

        hash = hash << 1 >> 1;

        ulong X = hash.hi + (hash.lo >> 32);
        X = ((X + (X >> 32) + 1) >> 32) + hash.hi;
        ulong Y = (X << 32) + hash.lo;

        ulong A = X + FINAL_MAGIC0;
        if (A < X) A += 0x101;

        ulong B = Y + FINAL_MAGIC1;
        if (B < Y) B += 0x101;

        UInt128 H = new UInt128(A) * B;
        UInt128 mul = new UInt128(0x101);
        H = (mul * H.hi) + H.lo;
        H = (mul * H.hi) + H.lo;

        if (H.hi > 0) H += mul;
        if (H.lo > 0xFFFFFFFFFFFFFEFE) H += mul;
        return H.lo;
    }

    static UInt128 hash_chunk(byte[] chunk, long size, int off)
    {
        UInt128 hash = new UInt128(0);
        for (int i = 0; i < 8; i++)
        {
            int offset = i * 16;
            if (offset >= size) break;
            ulong a = read_int64(chunk, off + offset);
            ulong b = read_int64(chunk, off + offset + 8);
            hash += (new UInt128(a + magic_table[i * 2])) * (new UInt128(b + magic_table[i * 2 + 1]));
        }
        return hash << 2 >> 2;
    }

    static UInt128 hash_muladd(UInt128 hash, UInt128 mul, UInt128 add)
    {
        ulong a0 = add.lo & 0xffffffff,
            a1 = add.lo >> 32,
            a23 = add.hi;

        ulong m0 = mul.lo & 0xffffffff,
            m1 = mul.lo >> 32,
            m2 = mul.hi & 0xffffffff,
            m3 = mul.hi >> 32;

        ulong h0 = hash.lo & 0xffffffff,
            h1 = hash.lo >> 32,
            h2 = hash.hi & 0xffffffff,
            h3 = hash.hi >> 32;

        ulong c0 = (h0 * m0),
            c1 = (h0 * m1) + (h1 * m0),
            c2 = (h0 * m2) + (h1 * m1) + (h2 * m0),
            c3 = (h0 * m3) + (h1 * m2) + (h2 * m1) + (h3 * m0),
            c4 = (h1 * m3) + (h2 * m2) + (h3 * m1),
            c5 = (h2 * m3) + (h3 * m2),
            c6 = (h3 * m3);

        ulong r2 = c2 + (c6 << 1) + a23,
            r3 = c3 + (r2 >> 32),
            r0 = c0 + (c4 << 1) + a0 + (r3 >> 31),
            r1 = c1 + (c5 << 1) + a1 + (r0 >> 32);

        ulong res0 = ((r3 << 33 >> 1) | (r2 & 0xffffffff)) + (r1 >> 32);
        return new UInt128(res0, (r1 << 32) | (r0 & 0xffffffff));
    }
}