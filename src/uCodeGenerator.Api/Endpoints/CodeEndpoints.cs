using Carter;
using uCodeGenerator.Core.Services;
using uCodeGenerator.Core.Utils;
using Microsoft.AspNetCore.Mvc; // for [FromQuery] types if we want later
using Microsoft.AspNetCore.Http;

namespace uCodeGenerator.Api.Endpoints;

public class CodeEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // 1. Validate full 13-char code (checksum etc.)
        app.MapGet("/validate", ([FromQuery] string code) =>
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != 13)
                return Results.BadRequest("code is required and must be 13 characters long");

            var prefix = code[..2];

            // check prefix allowed chars
            if (!UCodeGeneratorService.AllowedPrefixChars.Contains(char.ToUpperInvariant(prefix[0])) ||
                !UCodeGeneratorService.AllowedPrefixChars.Contains(char.ToUpperInvariant(prefix[1])))
            {
                return Results.BadRequest("prefix contains invalid characters");
            }

            // recompute checksum
            var withoutChecksum = code[..12];
            var expectedChecksum = Helper.ComputeChecksum(withoutChecksum);

            var lastChar = code[12];
            if (!char.IsDigit(lastChar))
                return Results.BadRequest("invalid checksum char");

            var actualChecksum = lastChar - '0';

            if (expectedChecksum != actualChecksum)
                return Results.BadRequest("invalid checksum");

            return Results.Ok(new { valid = true });
        });

        // 2. Validate parameters separately
        app.MapGet("/validate-params", (
            [FromQuery] string? prefix,
            [FromQuery] int? count,
            [FromQuery] string? year) =>
        {
            var errors = new List<string>();

            var prefixErr = Helper.ValidatePrefix(prefix, UCodeGeneratorService.AllowedPrefixChars);
            if (prefixErr is not null) errors.Add(prefixErr);

            var countErr = Helper.ValidateCount(count);
            if (countErr is not null) errors.Add(countErr);

            var yearErr = Helper.ValidateYear(year);
            if (yearErr is not null) errors.Add(yearErr);

            return Results.Ok(new { errors });
        });

        // 3. Stream CSV of generated codes
        app.MapGet("/generate-csv", async (
            [FromQuery] string prefix,
            [FromQuery] int count,
            [FromQuery] string year,
            HttpResponse response,
            UCodeGeneratorService generator) =>
        {
            // validation using new helpers
            var prefixErr = Helper.ValidatePrefix(prefix, UCodeGeneratorService.AllowedPrefixChars);
            if (prefixErr is not null)
                return Results.BadRequest(prefixErr);

            var countErr = Helper.ValidateCount(count);
            if (countErr is not null)
                return Results.BadRequest(countErr);

            var yearErr = Helper.ValidateYear(year);
            if (yearErr is not null)
                return Results.BadRequest(yearErr);

            response.Headers.ContentDisposition = "attachment; filename=codes.csv";
            response.ContentType = "text/csv";

            await using var writer = new StreamWriter(response.Body);

            for (int i = 0; i < count; i++)
            {
                var code = generator.GenerateCode(prefix, year);
                await writer.WriteLineAsync(code);
            }

            await writer.FlushAsync();

            return Results.Empty;
        });
    }
}
