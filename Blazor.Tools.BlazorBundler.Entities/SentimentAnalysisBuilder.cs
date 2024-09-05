using Microsoft.ML;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public class SentimentAnalysisModelBuilder
    {
        public void BuildAndSaveModel()
        {
            // Define file paths
            var dataPath = "Data/sentiment-data.csv";
            var modelPath = "MLModels/SentimentAnalysisModel.zip";

            // ML.NET context
            var mlContext = new MLContext(seed: 0);

            // Load data
            var data = mlContext.Data.LoadFromTextFile<SentimentData>(dataPath, separatorChar: ',', hasHeader: true);

            // Data processing pipeline
            var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.SentimentText))
                .Append(mlContext.Transforms.CopyColumns("Label", nameof(SentimentData.Sentiment)));

            // Choose a learning algorithm
            var trainer = mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features");

            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // Train the model
            var trainedModel = trainingPipeline.Fit(data);

            // Save the model
            mlContext.Model.Save(trainedModel, data.Schema, modelPath);
        }
    }

}
