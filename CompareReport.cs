static class CompareReport
{
    public static void Run(string[] args)
    {
        if (args.Length != 2)
            throw new FormatException("Usage: compare study.tsv");
        var rows = Rows(args[1]);
        Require(rows[0].Length == 1, "First row is observation end.");
        var end = Integer(rows[0][0]);
        Require(end > 0, "End must be positive.");
        var tracks = rows.Skip(1).Select(r =>
        {
            Require(r.Length == 3, "Observer, time, state required.");
            return (Who: r[0], At: Integer(r[1]), State: r[2]);
        }).GroupBy(x => x.Who).ToArray();
        Require(tracks.Length == 2, "Provide two observers.");
        var logs = tracks.Select(g => g.ToDictionary(x => x.At, x => x.State)).ToArray();
        Require(logs.All(log => log.ContainsKey(0) && log.Keys.All(t => t < end)), "Both tracks must start at zero and end before the stop.");
        var cuts = logs[0].Keys.Union(logs[1].Keys).Append(end).Order().ToArray();
        string a = "", b = "";
        long agreed = 0;
        for (int i = 0; i < cuts.Length - 1; i++)
        {
            long at = cuts[i], until = cuts[i + 1];
            a = logs[0].GetValueOrDefault(at, a);
            b = logs[1].GetValueOrDefault(at, b);
            if (a == b)
                agreed += until - at;
            else
                Console.WriteLine($"{at}–{until} s: {tracks[0].Key} = {a}, {tracks[1].Key} = {b}");
        }

        Console.WriteLine($"Time agreement: {agreed}/{end} s ({100m * agreed / end:0.##}%)");
    }

    static List<string[]> Rows(string file, int width = 0, bool allowEmpty = false)
    {
        using var reader = file == "-" ? Console.In : File.OpenText(file);
        var rows = new List<string[]>();
        int line = 0;
        while (reader.ReadLine()is string s)
        {
            line++;
            if (string.IsNullOrWhiteSpace(s) || s.TrimStart().StartsWith("#"))
                continue;
            var cells = s.Split('\t', StringSplitOptions.TrimEntries);
            if ((width > 0 && cells.Length != width) || (!allowEmpty && cells.Any(string.IsNullOrEmpty)))
                throw new FormatException($"Invalid columns on line {line}.");
            rows.Add(cells);
        }

        if (rows.Count == 0)
            throw new FormatException("Empty input.");
        return rows;
    }

    static void Require(bool condition, string message)
    {
        if (!condition)
            throw new FormatException(message);
    }

    static long Integer(string text)
    {
        var value = long.Parse(text);
        Require(value >= 0, "Count must be nonnegative.");
        return value;
    }
}
