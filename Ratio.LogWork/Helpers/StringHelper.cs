namespace Ratio.LogWork.Helpers
{
    public static class StringHelper
    {
        public static string ToCapitalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;
            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}
