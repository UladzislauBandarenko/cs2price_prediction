using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using cs2price_prediction.Config;

namespace cs2price_prediction.Services.AI.Llm
{
    /// <summary>
    /// Implementation of ILLMClient using the OpenAI Responses API (/v1/responses).
    /// Returns a single cleaned text answer without any newline characters (plain text).
    /// </summary>
    public class OpenAiClient : ILLMClient
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAiOptions _options;

        public OpenAiClient(HttpClient httpClient, OpenAiOptions options)
        {
            _httpClient = httpClient;
            _options = options;

            // Fail fast — API key must be provided
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
                throw new InvalidOperationException("OpenAI API key is not configured.");

            // Configure Authorization header once
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        /// <summary>
        /// Sends a prompt to the OpenAI Responses API and returns the extracted plain text output.
        /// If modelOverride is provided, it overrides the model from configuration.
        /// </summary>
        public async Task<string> QueryAsync(string prompt, string? modelOverride = null)
        {
            // Resolve base URL — custom value or official OpenAI endpoint
            var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl)
                ? "https://api.openai.com/v1"
                : _options.BaseUrl;

            var url = $"{baseUrl.TrimEnd('/')}/responses";

            // System prompt — ensures fully controlled, constrained, safe model behavior
            var systemInstructions =
                "You are an AI assistant that explains why a machine-learning model predicted a certain CS2 skin price. " +
                "You always receive a textual description that may include: weapon name, skin name, wear tier, float value, pattern or phase or fade percentage, blue score information, sticker slot prices and names, and the predicted_price value. " +
                "Your task is to explain in simple terms which of these factors most likely increase or decrease the predicted price. " +
                "RULES: " +
                "1) Answer ONLY in English. " +
                "2) Use ONLY the information explicitly present in the prompt. Do NOT invent sticker names, teams, tournaments, years, events, websites, or external market data. " +
                "3) Treat float as: lower value means better condition and usually higher price; higher value means more wear and usually lower price. " +
                "4) If all sticker slot prices are zero or sticker names are missing/empty, say that stickers do not affect the price, or simply do not mention stickers. Never assume that a knife has stickers unless the prompt explicitly includes sticker data. " +
                "5) Do NOT mention real trading platforms, marketplaces, or specific market statistics. Focus only on the relative influence of the given features. " +
                "6) The final answer must be 3–6 plain sentences with no lists, markdown, bullets, or formatting. " +
                "7) Output must be plain text only, without newline characters.";

            // Choose model: override → configured → fallback
            var modelName =
                !string.IsNullOrWhiteSpace(modelOverride)
                    ? modelOverride
                    : (!string.IsNullOrWhiteSpace(_options.PrimaryModel)
                        ? _options.PrimaryModel
                        : "gpt-4o-mini");

            var requestBody = new ResponsesRequest
            {
                Model = modelName,
                Instructions = systemInstructions,
                Input = prompt,
                Temperature = 0.1
            };

            // Send request to OpenAI
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"OpenAI error {(int)response.StatusCode} {response.ReasonPhrase}. Body: {errorBody}");
            }

            var rawJson = await response.Content.ReadAsStringAsync();

            // Parse JSON manually because output structure may vary
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            string? extractedText = null;

            // Try path 1: "output_text"
            if (root.TryGetProperty("output_text", out var outputTextProp) &&
                outputTextProp.ValueKind == JsonValueKind.String)
            {
                var txt = outputTextProp.GetString();
                if (!string.IsNullOrWhiteSpace(txt))
                    extractedText = txt;
            }

            // Try path 2: output[0].content[0].text / value
            if (extractedText is null &&
                root.TryGetProperty("output", out var outputProp) &&
                outputProp.ValueKind == JsonValueKind.Array &&
                outputProp.GetArrayLength() > 0)
            {
                var firstOutput = outputProp[0];

                if (firstOutput.TryGetProperty("content", out var contentProp) &&
                    contentProp.ValueKind == JsonValueKind.Array &&
                    contentProp.GetArrayLength() > 0)
                {
                    var firstContent = contentProp[0];

                    if (firstContent.TryGetProperty("text", out var textProp))
                    {
                        // Case A: text is a simple string
                        if (textProp.ValueKind == JsonValueKind.String)
                        {
                            var s = textProp.GetString();
                            if (!string.IsNullOrWhiteSpace(s))
                                extractedText = s;
                        }
                        // Case B: text is an object with "value"
                        else if (textProp.ValueKind == JsonValueKind.Object &&
                                 textProp.TryGetProperty("value", out var valueProp) &&
                                 valueProp.ValueKind == JsonValueKind.String)
                        {
                            var s = valueProp.GetString();
                            if (!string.IsNullOrWhiteSpace(s))
                                extractedText = s;
                        }
                        // Case C: text is an array → use the first element
                        else if (textProp.ValueKind == JsonValueKind.Array &&
                                 textProp.GetArrayLength() > 0)
                        {
                            var firstTextItem = textProp[0];

                            if (firstTextItem.ValueKind == JsonValueKind.String)
                            {
                                var s = firstTextItem.GetString();
                                if (!string.IsNullOrWhiteSpace(s))
                                    extractedText = s;
                            }
                            else if (firstTextItem.ValueKind == JsonValueKind.Object &&
                                     firstTextItem.TryGetProperty("value", out var v2) &&
                                     v2.ValueKind == JsonValueKind.String)
                            {
                                var s = v2.GetString();
                                if (!string.IsNullOrWhiteSpace(s))
                                    extractedText = s;
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                return $"Failed to parse LLM response. Raw JSON: {rawJson}";
            }

            // Cleanup: remove newlines, tabs, duplicated spaces
            var text = extractedText
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ");

            while (text.Contains("  "))
                text = text.Replace("  ", " ");

            return text.Trim();
        }

        /// <summary>
        /// DTO for OpenAI /v1/responses request.
        /// </summary>
        private sealed class ResponsesRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("input")]
            public string Input { get; set; } = string.Empty;

            [JsonPropertyName("instructions")]
            public string? Instructions { get; set; }

            [JsonPropertyName("temperature")]
            public double Temperature { get; set; } = 0.1;
        }
    }
}
