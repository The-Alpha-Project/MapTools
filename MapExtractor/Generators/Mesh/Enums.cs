// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;

namespace AlphaCoreExtractor.Generators.Mesh
{
    public enum PlaneIntersectionType
    {
        Front,
        Back,
        Intersecting
    }

    [Flags]
    public enum IntersectionType
    {
        NoIntersection = 0,
        Intersects = 1,
        Contained = 2,
        Touches = Intersects | Contained
    }

    public enum ContainmentType
    {
        Disjoint,
        Contains,
        Intersects
    }

    [Flags]
    enum OutFlags : byte
    {
        None = 0x00,
        Top = 0x01,
        Bottom = 0x02,
        Left = 0x04,
        Right = 0x08
    }

    public enum ClipperResponse
    {
        NoneClipped,
        SomeClipped,
        AllClipped
    }
}
