using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
if (args.Length > 0 && args[0] == "compare")
{
    try { CompareReport.Run(args); return 0; }
    catch (Exception e) when (e is FormatException or IOException or ArgumentException or OverflowException or UnauthorizedAccessException)
    { Console.Error.WriteLine(e.Message); return 1; }
}

if (args.Length != 1 || args[0] == "--help")
{
    Console.WriteLine("Usage: dotnet run -- compare study.tsv  |  original format: <input-file|->");
    return args.Length == 1 ? 0 : 2;
}

try
{
    string[] lines = args[0] == "-" ? Console.In.ReadToEnd().ReplaceLineEndings("\n").TrimEnd('\n').Split('\n') : File.ReadAllLines(args[0]);
    Check(lines.Length >= 2 && lines.Any(line => !string.IsNullOrWhiteSpace(line)), "Input is empty or missing records.");
    var end = Whole(lines[0], 1);
    var changes = lines.Skip(1).Select(s => Fields(s, 2)).Select(f => (Start: Whole(f[0], 0), Label: f[1])).ToArray();
    Check(changes.Length > 0 && changes[0].Start == 0, "Start the observation at zero.");
    var totals = new Dictionary<string, long>();
    for (int i = 0; i < changes.Length; i++)
    {
        var next = i + 1 < changes.Length ? changes[i + 1].Start : end;
        Check(next > changes[i].Start && next <= end, "Invalid time sequence.");
        totals[changes[i].Label] = totals.GetValueOrDefault(changes[i].Label) + next - changes[i].Start;
    }

    foreach (var(label, seconds)in totals.OrderByDescending(t => t.Value))
        Console.WriteLine($"{label}: {seconds} s ({100m * seconds / end:0.#}%)");
    return 0;
}
catch (Exception e)when (e is IOException or FormatException or OverflowException or ArgumentException or UnauthorizedAccessException or DivideByZeroException)
{
    Console.Error.WriteLine(e.Message);
    return 1;
}

static void Check(bool valid, string message)
{
    if (!valid)
        throw new FormatException(message);
}

static string[] Fields(string line, int count)
{
    var fields = line.Split('\t', StringSplitOptions.TrimEntries);
    Check(fields.Length == count && fields.All(s => s.Length > 0), $"Expected {count} tab-separated values in '{line}'.");
    return fields;
}

static long Whole(string value, long minimum)
{
    var number = long.Parse(value, CultureInfo.InvariantCulture);
    Check(number >= minimum, $"Expected an integer of at least {minimum}.");
    return number;
}
