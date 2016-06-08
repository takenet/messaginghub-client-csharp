namespace Takenet.MessagingHub.Client.Tester
{
    public static class StringExtensions
    {
        public static string ToStringFormatRegex(this string text, int formatPlaceholderCount = 10)
        {
            // Olá, {0}. Não se esqueça da sua consulta.     =>     (\W|^)Olá, [\w ]*[^\W_][\w ]*. Não se esqueça da sua consulta.(\W|$)

            for (var i = 0; i < formatPlaceholderCount; i++)
                text = text.Replace($"{{{i}}}", "[\\w ]*[^\\W_][\\w ]*");

            text = $"(\\W|^){text}(\\W|$)";
            return text;
        }
    }
}