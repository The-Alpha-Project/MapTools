// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using WoWFormatParser.Structures.Common;

namespace MapTools.Generators
{
    public static class Extensions
    {
        public static void WriteMapVersion(this FileStream fs)
        {
            var assembly = Assembly.GetExecutingAssembly().GetName();
            var version = $"ACMAP_{assembly.Version.Major}.{assembly.Version.Build}0";
            fs.Write(Encoding.ASCII.GetBytes(version), 0, 10);
        }

        public static IEnumerable<C3Vector> ToRecastVerts(this IList<C3Vector> vertices)
        {
            return vertices.Select(t => t.ToRecastVert());
        }

        public static IEnumerable<C3Vector> ToWowVerts(this IList<C3Vector> vertices)
        {
            return vertices.Select(t => t.ToWowVert());
        }

        public static C3Vector ToRecastVert(this C3Vector vertex)
        {
            return new C3Vector(vertex.Y, vertex.Z, vertex.X);
        }

        public static C3Vector ToWowVert(this C3Vector vertex)
        {
            return new C3Vector(vertex.Z, vertex.X, vertex.Y);
        }

        public const float Epsilon = 1e-6f;

        public static bool IsWithinEpsilon(this float val, float otherVal)
        {
            return ((val <= (otherVal + Epsilon)) && (val >= (otherVal - Epsilon)));
        }
    }
}
