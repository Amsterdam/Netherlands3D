namespace subtree
{

    public static class LevelOffset
    {
        public static int GetLevelOffset(int level, ImplicitSubdivisionScheme scheme = ImplicitSubdivisionScheme.Quadtree)
        {
            var result = scheme == ImplicitSubdivisionScheme.Quadtree ?
                ((1 << (2 * level)) - 1) / 3 :
                ((1 << (3 * level)) - 1) / 7;
            return result;
        }

        public static int GetNumberOfLevels(string availability, ImplicitSubdivisionScheme scheme = ImplicitSubdivisionScheme.Quadtree)
        {
            var level = 0;
            var length = availability.Length;
            var cont = true;

            while (cont)
            {
                var offset = GetLevelOffset(level, scheme);
                var offsetnext = GetLevelOffset(level + 1, scheme);

                if (offset < length && offsetnext > length)
                {
                    cont = false;
                }
                else
                {
                    level++;
                }
            }

            return level;
        }
    }
}
