using game2.game;

namespace game2Forms
{
    public static class Images
    {
        private static readonly List<Image> _resources = [];

        public static readonly Image Attack;
        public static readonly Image Defense;
        public static readonly Image HP;
        public static readonly Image Move;
        public static readonly Image Vision;

        public static Image[] Resources => [.. _resources];

        static Images()
        {
            Attack = Properties.Resources.Attack;
            Defense = Properties.Resources.Defense;
            HP = Properties.Resources.HP;
            Move = Properties.Resources.Move;
            Vision = Properties.Resources.Vision;

            LoadResources();
        }
        private static void LoadResources()
        {
            string[] resourceNames =
            [
                "Basic",
                "Advanced",
                "Mobility",
                "Special",
                "Research",
                "Upkeep",
            ];
            Color[] resourceColors = [
                Color.PaleGreen,
                Color.Silver,
                Color.BlueViolet,
                Color.DarkCyan,
                Color.Crimson,
                Color.FromArgb(0xFAC400),
            ];

            //var b = Properties.Resources.Basic;

            var rm = Properties.Resources.ResourceManager;
            for (int a = 0; a < resourceNames.Length; a++)
            {
                object? obj = rm.GetObject(resourceNames[a]);
                if (obj is Image image)
                {
                    int padding = Game.Rand.Round((image.Width + image.Height) / 10f);
                    Bitmap bitmap = new(image.Width + padding, image.Height + padding);
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.Clear(resourceColors[a]);
                        g.DrawImage(image, padding / 2f, padding / 2f, image.Width, image.Height);
                    }
                    _resources.Add(bitmap);
                }
                else
                {
                    // Resource not found — decide whether to log, throw, or skip.
                    //load an error image?
                }
            }
        }
    }
}
