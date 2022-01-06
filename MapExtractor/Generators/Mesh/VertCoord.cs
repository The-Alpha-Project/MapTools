// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;

namespace AlphaCoreExtractor.Generators.Mesh
{
    public struct VertCoord : IEquatable<VertCoord>
    {
        public readonly int I;
        public readonly int J;
        public readonly int K;

        public VertCoord(int i, int j, int k) : this()
        {
            K = k;
            J = j;
            I = i;
        }

        public bool Equals(VertCoord other)
        {
            return other.I == I && other.J == J && other.K == K;
        }

        public static bool operator ==(VertCoord v1, VertCoord v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(VertCoord v1, VertCoord v2)
        {
            return !(v1.Equals(v2));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(VertCoord)) return false;
            return Equals((VertCoord)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = I;
                result = (result * 397) ^ J;
                result = (result * 397) ^ K;
                return result;
            }
        }
    }
}
