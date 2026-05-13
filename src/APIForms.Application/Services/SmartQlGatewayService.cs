using System.Text.RegularExpressions;
using APIForms.Application.Interfaces;

namespace APIForms.Application.Services;

public sealed class SmartQlGatewayService(ISmartQlPolicyRepository policies)
{
    public async Task<Dictionary<string, object?>> ExecuteAsync(
        string tenantId,
        string formId,
        string eventName,
        Dictionary<string, object?> input,
        CancellationToken ct)
    {
        var active = await policies.ListByEventAsync(tenantId, formId, eventName, ct);
        if (active.Count == 0)
        {
            return input;
        }

        var working = new Dictionary<string, object?>(input, StringComparer.OrdinalIgnoreCase);

        foreach (var policy in active)
        {
            ExecuteScript(policy.Script, working);
        }

        return working;
    }

    private static void ExecuteScript(string script, Dictionary<string, object?> data)
    {
        var lines = script.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.StartsWith("//", StringComparison.Ordinal) || line.StartsWith("#", StringComparison.Ordinal))
                continue;

            if (line.StartsWith("ON ", StringComparison.OrdinalIgnoreCase))
                continue;

            if (line.StartsWith("RETURN", StringComparison.OrdinalIgnoreCase))
                return;

            if (line.StartsWith("REQUIRE ", StringComparison.OrdinalIgnoreCase))
            {
                var fieldPath = line[8..].Trim();
                var value = ResolvePath(data, fieldPath);
                if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
                    throw new InvalidOperationException($"Field '{fieldPath}' is required by SmartQL policy.");
                continue;
            }

            if (line.StartsWith("SET ", StringComparison.OrdinalIgnoreCase))
            {
                ApplySet(data, line[4..].Trim());
                continue;
            }

            if (line.StartsWith("IF ", StringComparison.OrdinalIgnoreCase))
            {
                ApplyIfReject(data, line);
                continue;
            }

            if (line.StartsWith("REJECT ", StringComparison.OrdinalIgnoreCase))
            {
                var reason = ExtractQuoted(line[7..].Trim()) ?? "Rejected by SmartQL policy.";
                throw new InvalidOperationException(reason);
            }
        }
    }

    private static void ApplySet(Dictionary<string, object?> data, string expr)
    {
        var parts = expr.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2) throw new InvalidOperationException($"Invalid SET expression: {expr}");

        var targetPath = parts[0].Trim();
        var rhs = parts[1].Trim();
        object? value;

        if (rhs.StartsWith("UPPER(", StringComparison.OrdinalIgnoreCase) && rhs.EndsWith(')'))
        {
            var arg = rhs[6..^1].Trim();
            value = (ResolvePath(data, arg)?.ToString() ?? string.Empty).ToUpperInvariant();
        }
        else if (rhs.StartsWith("LOWER(", StringComparison.OrdinalIgnoreCase) && rhs.EndsWith(')'))
        {
            var arg = rhs[6..^1].Trim();
            value = (ResolvePath(data, arg)?.ToString() ?? string.Empty).ToLowerInvariant();
        }
        else if (rhs.StartsWith("TRIM(", StringComparison.OrdinalIgnoreCase) && rhs.EndsWith(')'))
        {
            var arg = rhs[5..^1].Trim();
            value = (ResolvePath(data, arg)?.ToString() ?? string.Empty).Trim();
        }
        else if (rhs.StartsWith('"') && rhs.EndsWith('"'))
        {
            value = rhs[1..^1];
        }
        else
        {
            value = ResolvePath(data, rhs);
        }

        SetPath(data, targetPath, value);
    }

    private static void ApplyIfReject(Dictionary<string, object?> data, string line)
    {
        var notMatch = Regex.Match(line, "^IF\\s+(.+?)\\s+NOT_MATCH\\s+/(.+?)/\\s+THEN\\s+REJECT\\s+\"(.+)\"$", RegexOptions.IgnoreCase);
        if (notMatch.Success)
        {
            var fieldPath = notMatch.Groups[1].Value.Trim();
            var pattern = notMatch.Groups[2].Value;
            var message = notMatch.Groups[3].Value;
            var value = ResolvePath(data, fieldPath)?.ToString() ?? string.Empty;
            if (!Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase))
                throw new InvalidOperationException(message);
            return;
        }

        var isEmpty = Regex.Match(line, "^IF\\s+(.+?)\\s+IS_EMPTY\\s+THEN\\s+REJECT\\s+\"(.+)\"$", RegexOptions.IgnoreCase);
        if (isEmpty.Success)
        {
            var fieldPath = isEmpty.Groups[1].Value.Trim();
            var message = isEmpty.Groups[2].Value;
            var value = ResolvePath(data, fieldPath);
            if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
                throw new InvalidOperationException(message);
            return;
        }

        throw new InvalidOperationException($"Invalid IF expression: {line}");
    }

    private static string? ExtractQuoted(string value)
    {
        if (value.StartsWith('"') && value.EndsWith('"') && value.Length >= 2)
            return value[1..^1];
        return null;
    }

    private static object? ResolvePath(Dictionary<string, object?> data, string path)
    {
        var normalized = NormalizePath(path);
        return data.TryGetValue(normalized, out var value) ? value : null;
    }

    private static void SetPath(Dictionary<string, object?> data, string path, object? value)
    {
        var normalized = NormalizePath(path);
        data[normalized] = value;
    }

    private static string NormalizePath(string path)
    {
        var trimmed = path.Trim();
        if (trimmed.StartsWith("input.", StringComparison.OrdinalIgnoreCase))
            return trimmed[6..];
        return trimmed;
    }
}
