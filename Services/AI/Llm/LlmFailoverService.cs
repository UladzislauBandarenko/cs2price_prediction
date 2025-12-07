using System;
using System.Threading.Tasks;
using cs2price_prediction.Config;

namespace cs2price_prediction.Services.AI.Llm
{

    public class LlmFailoverService : ILlmFailoverService
    {
        private readonly ILLMClient _client;
        private readonly OpenAiOptions _options;

        public LlmFailoverService(ILLMClient client, OpenAiOptions options)
        {
            _client = client;
            _options = options;
        }

        public async Task<string> QueryWithPriorityAsync(string prompt, LlmPriority priority)
        {
            var primaryConfigured = string.IsNullOrWhiteSpace(_options.PrimaryModel)
                ? "gpt-4o-mini"
                : _options.PrimaryModel;

            var fallbackConfigured = string.IsNullOrWhiteSpace(_options.FallbackModel)
                ? "gpt-4.1-mini"
                : _options.FallbackModel;

            string primaryModel;
            string fallbackModel;

            if (priority == LlmPriority.MiniThenGpt41)
            {
                primaryModel = primaryConfigured;
                fallbackModel = fallbackConfigured;
            }
            else 
            {
                primaryModel = fallbackConfigured;
                fallbackModel = primaryConfigured;
            }

            try
            {
                return await _client.QueryAsync(prompt, primaryModel);
            }
            catch (Exception)
            {
                return await _client.QueryAsync(prompt, fallbackModel);
            }
        }
    }
}
