using Blazor.Tools.BlazorBundler.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.ML;

namespace Blazor.Tools.BlazorBundler.Components.AI
{
    public partial class AIChatBot : ComponentBase
    {
        [Parameter] public string ConnectionString { get; set; } = default!;

        private string _userInput = string.Empty;
        private List<Message> _messages = new List<Message>();
        private PredictionEngine<SentimentData, SentimentPrediction> _sentimentPredictionEngine = default!;
        private PredictionEngine<LanguageData, LanguagePrediction> _languagePredictionEngine = default!;
        private AIDataAccess _da = default!;

        protected override async Task OnInitializedAsync()
        {
            _da = new AIDataAccess(ConnectionString);
            _da.TrainingProgressUpdated += OnTrainingProgressUpdated;
            await InitializePredictionEngines();
            await base.OnInitializedAsync();
        }

        private async Task InitializePredictionEngines()
        {
            var mlContext = new MLContext();
            var sentimentModelData = await _da.GetModelZipFileAsync("SentimentAnalysisModel.zip");
            if (sentimentModelData.Length > 0)
            {
                using var sentimentModelStream = new MemoryStream(sentimentModelData);
                var sentimentModel = mlContext.Model.Load(sentimentModelStream, out var sentimentSchema);
                _sentimentPredictionEngine = mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(sentimentModel);
            }

            var languageModelData = await _da.GetModelZipFileAsync("LanguageAnalysisModel.zip");
            if (languageModelData.Length > 0)
            {
                using var languageModelStream = new MemoryStream(languageModelData);
                var languageModel = mlContext.Model.Load(languageModelStream, out var languageSchema);
                _languagePredictionEngine = mlContext.Model.CreatePredictionEngine<LanguageData, LanguagePrediction>(languageModel);
            }
        }

        private void OnTrainingProgressUpdated(string progressMessage)
        {
            _messages.Add(new Message { Text = progressMessage, IsBotMessage = true });
            InvokeAsync(StateHasChanged);
        }

        private async Task SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(_userInput))
            {
                _messages.Add(new Message { Text = _userInput, IsBotMessage = false });
                StateHasChanged();

                await HandleUserMessage(_userInput);

                _userInput = string.Empty;
            }
        }

        private async Task HandleUserMessage(string userMessage)
        {
            if (userMessage.ToLower().StartsWith("add info about"))
            {
                var parts = userMessage.Split(new[] { "add info about", ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var topic = parts[0].Trim();
                    var information = parts[1].Trim();
                    await AddGeneralInformation(topic, information);
                }
            }
            else if (userMessage.ToLower().StartsWith("what is"))
            {
                userMessage = RemovePunctuation(userMessage);
                var topic = userMessage.Substring("what is".Length).Trim();
                var information = await _da.GetGeneralInformationAsync(topic);
                if (information != null)
                {
                    _messages.Add(new Message { Text = information, IsBotMessage = true });
                }
                else
                {
                    _messages.Add(new Message { Text = "I don't have information about that topic.", IsBotMessage = true });
                }
                StateHasChanged();
            }
            else
            {
                if (_sentimentPredictionEngine != null)
                {
                    await ProcessSentimentMessageAsync(userMessage);
                }
                else if (_languagePredictionEngine != null)
                {
                    await ProcessLanguageMessageAsync(userMessage);
                }
                else
                {
                    _messages.Add(new Message { Text = "AI models are not initialized.", IsBotMessage = true });
                }

                StateHasChanged();
            }
        }

        private async Task ProcessSentimentMessageAsync(string userMessage)
        {
            var sentimentPrediction = _sentimentPredictionEngine.Predict(new SentimentData { SentimentText = userMessage });
            var sentimentResponse = sentimentPrediction.Prediction ? "Positive sentiment" : "Negative sentiment";
            var sentimentProbability = sentimentPrediction.Probability;
            var sentimentScore = sentimentPrediction.Score;

            _messages.Add(new Message { Text = sentimentResponse, IsBotMessage = true });

            var sentimentData = new SentimentData { SentimentText = userMessage, Sentiment = sentimentPrediction.Prediction };
            await _da.InsertSentimentDataAsync(sentimentData);

            var sentimentPredictionData = new SentimentPrediction
            {
                Prediction = sentimentPrediction.Prediction,
                Probability = sentimentProbability,
                Score = sentimentScore
            };

            await _da.InsertSentimentPredictionAsync(sentimentPredictionData);
        }

        private async Task ProcessLanguageMessageAsync(string userMessage)
        {
            var languagePrediction = _languagePredictionEngine.Predict(new LanguageData { Question = userMessage });
            var languageResponse = languagePrediction.Response ?? "I'm sorry, I don't understand that. Can you please rephrase?";
            var languageProbability = languagePrediction.Probability;
            var languageScore = languagePrediction.Score;

            _messages.Add(new Message { Text = languageResponse, IsBotMessage = true });

            var languagePredictionData = new LanguagePrediction
            {
                Response = languageResponse,
                Probability = languageProbability,
                Score = languageScore
            };

            await _da.InsertLanguagePredictionAsync(languagePredictionData);
        }

        private string RemovePunctuation(string text)
        {
            var punctuation = new char[] { '.', ',', '!', '?', ';' };
            while (text.Length > 0 && punctuation.Contains(text[^1]))
            {
                text = text.Remove(text.Length - 1);
            }
            return text.Trim();
        }

        private async Task TrainAIAsync()
        {
            var dataPath = "Data/sentiment-data.csv";

            _messages.Add(new Message { Text = "Sentiment model training has started...", IsBotMessage = true });
            StateHasChanged();

            try
            {
                await _da.TrainSentimentModelAsync(dataPath);
                await InitializePredictionEngines();
            }
            catch (Exception ex)
            {
                _messages.Add(new Message { Text = $"Error: {ex.Message}", IsBotMessage = true });
            }

            StateHasChanged();
        }

        private async Task TrainLanguageAIAsync()
        {
            var dataPath = "Data/language-data.csv";

            _messages.Add(new Message { Text = "Language model training has started...", IsBotMessage = true });
            StateHasChanged();

            try
            {
                await _da.TrainLanguageModelAsync(dataPath);
                await InitializePredictionEngines();
            }
            catch (Exception ex)
            {
                _messages.Add(new Message { Text = $"Error: {ex.Message}", IsBotMessage = true });
            }

            StateHasChanged();
        }

        private async Task AddGeneralInformation(string topic, string information)
        {
            try
            {
                await _da.InsertGeneralInformationAsync(topic, information);
                _messages.Add(new Message { Text = $"Information about {topic} added successfully!", IsBotMessage = true });
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _messages.Add(new Message { Text = $"Error: {ex.Message}", IsBotMessage = true });
                StateHasChanged();
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var sequence = 0;

            builder.OpenElement(sequence++, "h3");
            builder.AddContent(sequence++, "AI Chatbot");
            builder.CloseElement();

            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "row");

            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "column");
            builder.OpenElement(sequence++, "button");
            builder.AddAttribute(sequence++, "class", "btn btn-primary");
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, TrainAIAsync));
            builder.AddContent(sequence++, "Train AI");
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "column");
            builder.OpenElement(sequence++, "button");
            builder.AddAttribute(sequence++, "class", "btn btn-primary");
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, TrainLanguageAIAsync));
            builder.AddContent(sequence++, "Train Language AI");
            builder.CloseElement();
            builder.CloseElement();

            builder.CloseElement();

            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "row");

            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "col-md-12");
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "class", "chat-container");

            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "id", "message-container-messages");
            foreach (var message in _messages)
            {
                builder.OpenElement(sequence++, "div");
                builder.AddAttribute(sequence++, "class", message.IsBotMessage ? "bot-message" : "user-message");
                builder.AddContent(sequence++, message.Text);
                builder.CloseElement();
            }
            builder.CloseElement();

            builder.OpenElement(sequence++, "input");
            builder.AddAttribute(sequence++, "type", "text");
            builder.AddAttribute(sequence++, "value", _userInput);
            builder.AddAttribute(sequence++, "oninput", EventCallback.Factory.CreateBinder(this, value => _userInput = value ?? default!, _userInput));
            builder.AddAttribute(sequence++, "class", "form-control");
            builder.CloseElement();

            builder.OpenElement(sequence++, "button");
            builder.AddAttribute(sequence++, "class", "btn btn-primary");
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, SendMessage));
            builder.AddContent(sequence++, "Send");
            builder.CloseElement();

            builder.CloseElement();
            builder.CloseElement();

            builder.CloseElement();
        }
    }
}
