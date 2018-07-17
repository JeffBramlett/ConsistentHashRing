using Extensions.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ConsistentHashRing
{
    /// <summary>
    /// Executing static class to provide UInt32 hashcode for any object
    /// </summary>
    /// <remarks>
    /// UInt32 range is 0 to 4,294,967,295
    /// Use the fastest hashing that has the least duplicates
    /// Using MurmurHash.net from GIT Repro
    /// </remarks>
    public static class Hashing
    {
        #region Constants
        const UInt32 m = 0x5bd1e995;
        const Int32 r = 24;
        #endregion

        [StructLayout(LayoutKind.Explicit)]
        struct BytetoUInt32Converter
        {
            [FieldOffset(0)]
            public Byte[] Bytes;

            [FieldOffset(0)]
            public UInt32[] UInts;
        }

        /// <summary>
        /// Create the hashcode for the object
        /// </summary>
        /// <typeparam name="T">the type of the object</typeparam>
        /// <param name="item">the object</param>
        /// <returns></returns>
        public static UInt32 HashItem<T>(T item)
        {
            try
            {
                List<byte> byteList = new List<byte>();

                using (MemoryStream ms = new MemoryStream())
                {

                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(ms, item);

                    ms.Flush();
                    ms.Position = 0;

                    byte[] buffer = new byte[100];
                    while(true)
                    {
                        var numRead = ms.Read(buffer, 0, buffer.Length);
                        if (numRead == 0)
                            break;
                        else
                        {
                            for(var i = 0; i < numRead; i++)
                            {
                                byteList.Add(buffer[i]);
                            }
                        }
                    }
                }

                return Hash(byteList.ToArray());
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Make hashcode from arrary
        /// </summary>
        /// <param name="data">data array</param>
        /// <returns>UINt32 a.k.a. hashcode</returns>
        public static UInt32 Hash(Byte[] data)
        {
            return Hash(data, Convert.ToUInt32(data.Length));
        }

        /// <summary>
        /// Make Hashcode from data array and seed value
        /// </summary>
        /// <param name="data">the byte array for hash</param>
        /// <param name="seed">the seed value for hash</param>
        /// <returns></returns>
        public static UInt32 Hash(Byte[] data, UInt32 seed)
        {
            Int32 length = data.Length;
            if (length == 0)
                return 0;

            UInt32 h = seed ^ (UInt32)length;
            Int32 currentIndex = 0;

            // array will be length of Bytes but contains Uints
            // therefore the currentIndex will jump with +1 while length will jump with +4
            UInt32[] hackArray = new BytetoUInt32Converter { Bytes = data }.UInts;

            while (length >= 4)
            {
                UInt32 k = hackArray[currentIndex++];
                k *= m;
                k ^= k >> r;
                k *= m;

                h *= m;
                h ^= k;
                length -= 4;
            }
            currentIndex *= 4; // fix the length
            switch (length)
            {
                case 3:
                    h ^= (UInt16)(data[currentIndex++] | data[currentIndex++] << 8);
                    h ^= (UInt32)data[currentIndex] << 16;
                    h *= m;
                    break;
                case 2:
                    h ^= (UInt16)(data[currentIndex++] | data[currentIndex] << 8);
                    h *= m;
                    break;
                case 1:
                    h ^= data[currentIndex];
                    h *= m;
                    break;
                default:
                    break;
            }

            // Do a few final mixes of the hash to ensure the last few
            // bytes are well-incorporated.

            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;

            return h;
        }
    }
}
