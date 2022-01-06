// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

namespace AlphaCoreExtractor.Generators.Mesh
{
    public class CVertex
    {
        public Vector3 Point;
        public float Distance;
        public bool IsVisible;

        public CVertex(Vector3 point)
        {
            Distance = 0.0f;
            IsVisible = true;
            Point = point;
        }
    }
}
