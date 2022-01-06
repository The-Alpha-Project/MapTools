// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

namespace AlphaCoreExtractor.Generators.Mesh
{
    public class CFace
    {
        public VertCoord[] VertIdxs;
        public bool IsVisible;

        public CFace(VertCoord[] vertIdxs)
        {
            IsVisible = true;
            VertIdxs = vertIdxs;
        }
    }
}
