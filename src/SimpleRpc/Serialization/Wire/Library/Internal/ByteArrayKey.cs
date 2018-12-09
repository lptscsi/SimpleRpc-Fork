// -----------------------------------------------------------------------
//   <copyright file="ByteArrayKey.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

namespace SimpleRpc.Serialization.Wire.Library.Internal
{
    internal struct ByteArrayKey
    {
        public readonly byte[] Bytes;
        private readonly int _hashCode;

        public override bool Equals(object obj)
        {
            var other = (ByteArrayKey) obj;
            return Compare(Bytes, other.Bytes);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        private static int GetHashCode([NotNull] byte[] bytes)
        {
            unchecked
            {
                var hash = 17;
                for (var i = 0; i < bytes.Length; i++)
                {
                    hash = hash*23 + bytes[i];
                }
                return hash;
            }
        }

        public ByteArrayKey(byte[] bytes)
        {
            Bytes = bytes;
            _hashCode = GetHashCode(bytes);
        }

        public static ByteArrayKey Create(byte[] bytes)
        {
            return new ByteArrayKey(bytes);
        }

        public static bool Compare(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
            {
                return true;
            }
            if ((a1 != null) && (a2 != null))
            {
                if (a1.Length != a2.Length)
                {
                    return false;
                }
                for (int i = 0; i < a1.Length; i++)
                {
                    if (a1[i] != a2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}