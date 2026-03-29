namespace Flawls.API.Services
{
    public static class BarcodeService
    {
        private static readonly Random _rng = Random.Shared;

        public static string Generate()
        {
            var digits = new int[13];
            digits[0] = _rng.Next(1, 10); // first digit never 0
            for (int i = 1; i < 12; i++)
                digits[i] = _rng.Next(0, 10);

            int sum = 0;
            for (int i = 0; i < 12; i++)
                sum += digits[i] * (i % 2 == 0 ? 1 : 3);
            digits[12] = (10 - (sum % 10)) % 10;

            return string.Join("", digits);
        }
    }
}
