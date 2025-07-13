namespace Ratio.LogWork.Service.SimpleAutoComplete
{
    public class LogWorkAutoCompleteHandler : IAutoCompleteHandler
    {
        private readonly IEnumerable<string> _commands;

        public LogWorkAutoCompleteHandler(IEnumerable<string> commands)
        {
            _commands = commands;
        }

        public char[] Separators { get; set; } = new char[] { ' ' };

        public string[] GetSuggestions(string text, int index)
        {
            var results = new List<string>();
            foreach (var cmd in _commands)
            {
                if (cmd.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                    results.Add(cmd);
            }

            return results.ToArray();
        }
    }
}
