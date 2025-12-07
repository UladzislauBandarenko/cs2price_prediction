namespace cs2price_prediction.Config
{
    public class OpenAiOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";
        public string PrimaryModel { get; set; } = "gpt-4o-mini";
        public string FallbackModel { get; set; } = "gpt-4.1-mini";
        public string? Model { get; set; }
    }
}
