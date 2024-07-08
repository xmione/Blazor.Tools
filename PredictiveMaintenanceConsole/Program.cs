﻿
// This file was auto-generated by ML.NET Model Builder. 

using PredictiveMaintenanceConsole;

// Create single instance of sample data from first line of dataset for model input
PredictiveMaintenanceModel.ModelInput sampleData = new PredictiveMaintenanceModel.ModelInput()
{
    Product_ID = @"L47181",
    Type = @"L",
    Air_temperature_K = 298.2F,
    Process_temperature_K = 308.7F,
    Rotational_speed_rpm = 1408F,
    Torque_Nm = 46.3F,
    Tool_wear_min = 3F,
};



Console.WriteLine("Using model to make single prediction -- Comparing actual Machine_failure with predicted Machine_failure from sample data...\n\n");


Console.WriteLine($"Product_ID: {@"L47181"}");
Console.WriteLine($"Type: {@"L"}");
Console.WriteLine($"Air_temperature_K: {298.2F}");
Console.WriteLine($"Process_temperature_K: {308.7F}");
Console.WriteLine($"Rotational_speed_rpm: {1408F}");
Console.WriteLine($"Torque_Nm: {46.3F}");
Console.WriteLine($"Tool_wear_min: {3F}");
Console.WriteLine($"Machine_failure: {0F}");


var sortedScoresWithLabel = PredictiveMaintenanceModel.PredictAllLabels(sampleData);
Console.WriteLine($"{"Class",-40}{"Score",-20}");
Console.WriteLine($"{"-----",-40}{"-----",-20}");

foreach (var score in sortedScoresWithLabel)
{
    Console.WriteLine($"{score.Key,-40}{score.Value,-20}");
}

Console.WriteLine("=============== End of process, hit any key to finish ===============");
Console.ReadKey();
