namespace CoreLib
{
    public static class StringExtensions
    {
        public static string Bolded(this string str) => string.Format("<b>{0}</b>", str);

        public static string WithItalics(this string str) => string.Format("<i>{0}</i>", str);

        public static string ColoredRed(this string str) => string.Format("<color=red>{0}</color>", str);

        public static string ColoredBlue(this string str) => string.Format("<color=blue>{0}</color>", str);

        public static string ColoredGreen(this string str) => string.Format("<color=green>{0}</color>", str);

        public static string ColoredGray(this string str) => string.Format("<color=gray>{0}</color>", str);
    }
}