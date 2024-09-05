namespace Blazor.Tools.BlazorBundler.Extensions
{
    public static class DateTimeExtensions
    {
        public static string To_yyyy_MM_dd(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        public static string To_dd_MM_yyyy(this DateTime date)
        {
            return date.ToString("dd-MM-yyyy");
        }

        public static string To_MM_dd_yyyy(this DateTime date)
        {
            return date.ToString("MM-dd-yyyy");
        }

        public static string To_yyyy_MM_dd_HH_mm_ss(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string To_dd_MM_yyyy_HH_mm_ss(this DateTime date)
        {
            return date.ToString("dd-MM-yyyy HH:mm:ss");
        }

        public static string To_MM_dd_yyyy_HH_mm_ss(this DateTime date)
        {
            return date.ToString("MM-dd-yyyy HH:mm:ss");
        }

        public static string To_MMM_dd_yyyy(this DateTime date)
        {
            return date.ToString("MMM dd, yyyy");
        }

        public static string To_dd_MMM_yyyy(this DateTime date)
        {
            return date.ToString("dd MMM, yyyy");
        }

        public static string To_dd_MMMM_yyyy(this DateTime date)
        {
            return date.ToString("dd MMMM, yyyy");
        }
    }

}
