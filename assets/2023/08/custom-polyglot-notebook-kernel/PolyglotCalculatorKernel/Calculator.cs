namespace PolyglotCalculatorKernel;

public class Calculator
{
    public Dictionary<string, int> Variables { get; } = new();
    private readonly List<string> _data = new();

    public int Execute(string code)
    {
        _data.Clear();
        _data.AddRange(code.Split(' '));
        int result;
        if (_data.Count == 0) return 0;
        if (_data.Count > 2 && _data[1] == "=")
        {
            var name = _data[0];
            result = Evaluate(2);
            Variables[name] = result;
        }
        else
        {
            result = Evaluate(0);
        }
        return result;
    }

    private int Evaluate(int startIndex)
    {
        var index = startIndex;
        var result = GetInt(ref index);
        while (index < _data.Count - 1)
        {
            var op = _data[index++];
            result = op switch
            {
                "+" => result + GetInt(ref index),
                "-" => result - GetInt(ref index),
                "*" => result * GetInt(ref index),
                _ => throw new InvalidOperationException($"Unknown operator: {op}")
            };
        }
        return result;
    }

    private int GetInt(ref int index)
    {
        var item = _data[index++];
        return int.TryParse(item, out var value)
            ? value
            : GetVariable(item);
    }

    public static string GetTokenAtPosition(string code, int position, bool partialToken = false)
    {
        if (string.IsNullOrEmpty(code)) return string.Empty;
        var endPosition = partialToken ? position : code.IndexOf(' ', position + 1);
        if (endPosition == -1) endPosition = code.Length;
        var codeBeforeCursor = code[..endPosition];
        var tokensBeforeCursor = codeBeforeCursor.Split(' ');
        return tokensBeforeCursor[^1];
    }

    private int GetVariable(string name)
    {
        return Variables.TryGetValue(name, out var value)
            ? value
            : throw new InvalidOperationException($"Unknown Variable: {name}");
    }
}