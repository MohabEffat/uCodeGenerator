using Carter;
using CodeService;

namespace CodeApi.Endpoints
{
    public class CodeEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/validate", (string code) =>
            {
                if (string.IsNullOrWhiteSpace(code) || code.Length != 13)
                    return Results.BadRequest("Code is required and must be 13 characters long");

                var prefix = code[..2];

                // --- validation ---
                var validation = Helper.ValidatePrefix(prefix);

                if (!validation.IsValid)
                    return Results.BadRequest(validation.ErrorMessage);

                var numericValues = code[2..10];

                char checkSum = code[^1];

                if (!numericValues.All(char.IsDigit) || !char.IsDigit(checkSum))
                    return Results.BadRequest("Invalid numeric section or checksum.");

                // --- checkSum validation ---
                int expectedChecksum = Helper.CalculateChecksum(code[..^1]);
                int actualChecksum = checkSum - '0';

                if (expectedChecksum != actualChecksum)
                    return Results.BadRequest("Invalid checksum.");

                // UID validation
                var UIDs = code.Substring(10, 2);

                if (!UIDs.All(char.IsLetter))
                    return Results.BadRequest("Invalid UID formation.");

                var data = new
                {
                    code,
                    isValid = true
                };

                return Results.Ok(new { data, limits = new{ maxCsv = 1_000_000, maxJson = 10_000 }});
            });

            app.MapGet("/generate", (string prefix, int count, string? year) =>
            {
                year ??= "25";

                // --- validation ---
                var validation = Helper.ValidatePrefix(prefix);
                if (!validation.IsValid)
                    return Results.BadRequest(validation.ErrorMessage);

                var validationYear = Helper.ValidateYear(year);
                if (!validationYear.IsValid)
                    return Results.BadRequest(validationYear.ErrorMessage);

                if (count <= 0 || count > 10_000)
                    return Results.BadRequest("count must be between 1 and 10,000.");

                var generator = new uCodeGenerator();

                var codes = new List<string>(capacity: count);

                for (int i = 0; i < count; i++) {
                    codes.Add(generator.GenerateCode(prefix, year));
                }

                return Results.Ok(new { prefix, year, count, codes });
            });

            app.MapGet("/generate-csv", async (string prefix, int count, string? year, HttpResponse response) =>
            {
                year ??= "25";

                // --- validation ---
                var validation = Helper.ValidatePrefix(prefix);
                if (!validation.IsValid)
                    return Results.BadRequest(validation.ErrorMessage);

                var validationYear = Helper.ValidateYear(year);
                if (!validationYear.IsValid)
                    return Results.BadRequest(validationYear.ErrorMessage);

                if (count <= 0 || count > 1_000_000)
                    return Results.BadRequest("count must be between 1 and 1,000,000.");

                var generator = new uCodeGenerator();

                var fileName = $"codes_{prefix}_{year}_{count}.csv";

                // --- set headers ONCE and don't return Results.Text after ---
                response.ContentType = "text/csv";
                response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";

                // We'll always stream, even for small counts. Simpler + consistent.
                await using var writer = new StreamWriter(response.Body);

                for (int i = 0; i < count; i++)
                {
                    var code = generator.GenerateCode(prefix, year);
                    await writer.WriteLineAsync(code);
                }

                await writer.FlushAsync();

                // Important: return Results.Empty so we don't overwrite headers/body
                return Results.Empty;
            });
        }
    }
}
