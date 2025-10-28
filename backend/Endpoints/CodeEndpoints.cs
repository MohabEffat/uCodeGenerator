using Carter;

namespace uCodeGenerator.Endpoints
{
    public class CodeEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/validate", (string code) =>
            {
                var prefix = code.Substring(0, 2);

                var errors = new List<string>();

                if (code.Length != 13)
                    errors.Add("Code Length is invalid");

                // prefix validation
                if (string.IsNullOrWhiteSpace(prefix))
                    errors.Add("prefix is required");
                else if (prefix.Length != 2)
                    errors.Add("prefix must be exactly 2 characters");
                else if (!prefix.All(c => uCodeGenerator.AllowedPrefixChars.Contains(char.ToUpperInvariant(c))))
                    errors.Add("prefix contains invalid characters");

                // numeric values validation
                var numericValues = code.Substring(2, code.Length - 5);

                char checkSum = code[code.Length - 1];

                if (!numericValues.All(char.IsDigit) || !char.IsDigit(checkSum))
                {
                    errors.Add("Invalid numeric section or checksum.");
                }

                // UID validation

                var UIDs = code.Substring(10, 2);

                if (!UIDs.All(char.IsLetter))
                {
                    errors.Add("Invalid UID formation.");
                }

                var ok = errors.Count == 0;

                var data = new
                {
                    code,
                    isValid = ok
                };

                if (!ok)
                    return Results.BadRequest(new { errors, data });

                return Results.Ok(new { data, limits = new{ maxCsv = 1_000_000, maxJson = 10_000 }});
            });

            app.MapGet("/generate", (string prefix, int count, string? year) =>
            {
                if (string.IsNullOrWhiteSpace(prefix) || prefix.Length != 2)
                    return Results.BadRequest("prefix must be exactly 2 characters.");

                if (count <= 0 || count > 10_000)
                    return Results.BadRequest("count must be between 1 and 10,000.");

                var generator = new uCodeGenerator(year ?? "25");

                var codes = new List<string>(capacity: count);

                for (int i = 0; i < count; i++)
                    codes.Add(generator.GenerateCode(prefix));

                return Results.Ok(new { prefix, year = year ?? "25", count, codes });
            });

            app.MapGet("/generate-csv", async (string prefix, int count, string? year, HttpResponse response) =>
            {
                // --- validation ---
                if (string.IsNullOrWhiteSpace(prefix) || prefix.Length != 2)
                    return Results.BadRequest("prefix must be exactly 2 characters.");
                if (count <= 0 || count > 1_000_000)
                    return Results.BadRequest("count must be between 1 and 1,000,000.");

                var y = year ?? "25";
                var generator = new uCodeGenerator(y);

                var fileName = $"codes_{prefix}_{y}_{count}.csv";

                // --- set headers ONCE and don't return Results.Text after ---
                response.ContentType = "text/csv";
                response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";

                // We'll always stream, even for small counts. Simpler + consistent.
                await using var writer = new StreamWriter(response.Body);

                for (int i = 0; i < count; i++)
                {
                    var code = generator.GenerateCode(prefix);
                    await writer.WriteLineAsync(code);
                }

                await writer.FlushAsync();

                // Important: return Results.Empty so we don't overwrite headers/body
                return Results.Empty;
            });
        }
    }
}
