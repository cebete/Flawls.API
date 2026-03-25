namespace Flawls.API.Services
{
    public static class BarcodeService
    {
        private static readonly Random _rng = Random.Shared;
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string Generate()
        {
            var suffix = new char[6];
            for (int i = 0; i < suffix.Length; i++)
            {
                suffix[i] = Chars[_rng.Next(Chars.Length)];
            }

            return "FL" + new string(suffix);
        }
    }
}
