﻿// This file was auto-generated by ML.NET Model Builder.
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.LightGbm;
using Microsoft.ML.Transforms;

namespace PredictiveMaintenance
{
    public partial class PredictiveMaintenanceModel
    {
        public const string RetrainFilePath =  @"C:\repo\Blazor.Tools\PredictiveMaintenance\ai4i2020.csv";
        public const char RetrainSeparatorChar = ',';
        public const bool RetrainHasHeader =  true;
        public const bool RetrainAllowQuoting =  false;

         /// <summary>
        /// Train a new model with the provided dataset.
        /// </summary>
        /// <param name="outputModelPath">File path for saving the model. Should be similar to "C:\YourPath\ModelName.mlnet"</param>
        /// <param name="inputDataFilePath">Path to the data file for training.</param>
        /// <param name="separatorChar">Separator character for delimited training file.</param>
        /// <param name="hasHeader">Boolean if training file has a header.</param>
        public static void Train(string outputModelPath, string inputDataFilePath = RetrainFilePath, char separatorChar = RetrainSeparatorChar, bool hasHeader = RetrainHasHeader, bool allowQuoting = RetrainAllowQuoting)
        {
            var mlContext = new MLContext();

            var data = LoadIDataViewFromFile(mlContext, inputDataFilePath, separatorChar, hasHeader, allowQuoting);
            var model = RetrainModel(mlContext, data);
            SaveModel(mlContext, model, data, outputModelPath);
        }

        /// <summary>
        /// Load an IDataView from a file path.
        /// </summary>
        /// <param name="mlContext">The common context for all ML.NET operations.</param>
        /// <param name="inputDataFilePath">Path to the data file for training.</param>
        /// <param name="separatorChar">Separator character for delimited training file.</param>
        /// <param name="hasHeader">Boolean if training file has a header.</param>
        /// <returns>IDataView with loaded training data.</returns>
        public static IDataView LoadIDataViewFromFile(MLContext mlContext, string inputDataFilePath, char separatorChar, bool hasHeader, bool allowQuoting)
        {
            return mlContext.Data.LoadFromTextFile<ModelInput>(inputDataFilePath, separatorChar, hasHeader, allowQuoting: allowQuoting);
        }


        /// <summary>
        /// Save a model at the specified path.
        /// </summary>
        /// <param name="mlContext">The common context for all ML.NET operations.</param>
        /// <param name="model">Model to save.</param>
        /// <param name="data">IDataView used to train the model.</param>
        /// <param name="modelSavePath">File path for saving the model. Should be similar to "C:\YourPath\ModelName.mlnet.</param>
        public static void SaveModel(MLContext mlContext, ITransformer model, IDataView data, string modelSavePath)
        {
            // Pull the data schema from the IDataView used for training the model
            DataViewSchema dataViewSchema = data.Schema;

            using (var fs = File.Create(modelSavePath))
            {
                mlContext.Model.Save(model, dataViewSchema, fs);
            }
        }


        /// <summary>
        /// Retrain model using the pipeline generated as part of the training process.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <param name="trainData"></param>
        /// <returns></returns>
        public static ITransformer RetrainModel(MLContext mlContext, IDataView trainData)
        {
            var pipeline = BuildPipeline(mlContext);
            var model = pipeline.Fit(trainData);

            return model;
        }

        /// <summary>
        /// build the pipeline that is used from model builder. Use this function to retrain model.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <returns></returns>
        public static IEstimator<ITransformer> BuildPipeline(MLContext mlContext)
        {
            // Data process configuration with pipeline data transformations
            var pipeline = mlContext.Transforms.Categorical.OneHotEncoding(@"Type", @"Type", outputKind: OneHotEncodingEstimator.OutputKind.Indicator)      
                                    .Append(mlContext.Transforms.ReplaceMissingValues(new []{new InputOutputColumnPair(@"Air temperature K", @"Air temperature K"),new InputOutputColumnPair(@"Process temperature K", @"Process temperature K"),new InputOutputColumnPair(@"Rotational speed rpm", @"Rotational speed rpm"),new InputOutputColumnPair(@"Torque Nm", @"Torque Nm"),new InputOutputColumnPair(@"Tool wear min", @"Tool wear min")}))      
                                    .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName:@"Product ID",outputColumnName:@"Product ID"))      
                                    .Append(mlContext.Transforms.Concatenate(@"Features", new []{@"Type",@"Air temperature K",@"Process temperature K",@"Rotational speed rpm",@"Torque Nm",@"Tool wear min",@"Product ID"}))      
                                    .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName:@"Machine failure",inputColumnName:@"Machine failure",addKeyValueAnnotationsAsText:false))      
                                    .Append(mlContext.MulticlassClassification.Trainers.LightGbm(new LightGbmMulticlassTrainer.Options(){NumberOfLeaves=13,NumberOfIterations=123,MinimumExampleCountPerLeaf=20,LearningRate=0.21338081397513467,LabelColumnName=@"Machine failure",FeatureColumnName=@"Features",Booster=new GradientBooster.Options(){SubsampleFraction=0.9999997766729865,FeatureFraction=0.9932642926900449,L1Regularization=2.918384928728697E-10,L2Regularization=0.39698573593899295},MaximumBinCountPerFeature=312}))      
                                    .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName:@"PredictedLabel",inputColumnName:@"PredictedLabel"));

            return pipeline;
        }
    }
 }
