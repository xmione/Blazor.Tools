/*

USE master
DROP DATABASE AIDatabase
CREATE DATABASE AIDatabase


USE AIDatabase

CREATE TABLE TrainingData (
    ID INT PRIMARY KEY IDENTITY,
    SentimentText NVARCHAR(MAX),
    Sentiment BIT
)

CREATE TABLE ModelFiles (
    ID INT PRIMARY KEY IDENTITY,
    FileName NVARCHAR(255) UNIQUE,
    ModelData VARBINARY(MAX)
);

CREATE TABLE SentimentPredictions (
    ID INT PRIMARY KEY IDENTITY,
    Prediction BIT,
    Probability FLOAT,
    Score FLOAT
);

CREATE TABLE GeneralInformation (
    ID INT PRIMARY KEY IDENTITY,
    Topic NVARCHAR(255),
    Information NVARCHAR(MAX)
);

select * from TrainingData

select * from ModelFiles

select * from SentimentPredictions

select * from GeneralInformation

*/
