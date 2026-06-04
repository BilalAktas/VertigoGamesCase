namespace Core
{
    public static class LevelManager
    {
        private static int _level = 1;
        public static void IncreaseLevel() => _level++;
        public static int GetLevel() => _level;
        public static int ResetLevel() => _level = 1;
    }
}