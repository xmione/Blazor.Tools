/*



USE master
Go

DROP DATABASE AIDatabase
Go

CREATE DATABASE AIDatabase
Go

USE AIDatabase
Go

CREATE TABLE ModelFiles (
    ID INT PRIMARY KEY IDENTITY,
    FileName NVARCHAR(255) UNIQUE,
    ModelData VARBINARY(MAX)
)

Go

CREATE TABLE SentimentData (
    ID INT PRIMARY KEY IDENTITY,
    SentimentText NVARCHAR(MAX),
    Sentiment BIT
)

Go

CREATE TABLE SentimentPredictions (
    ID INT PRIMARY KEY IDENTITY,
    Prediction BIT,
    Probability FLOAT,
    Score FLOAT
)

Go

CREATE TABLE LanguageData (
    ID INT PRIMARY KEY IDENTITY,
    Question NVARCHAR(255) NOT NULL,
    Response NVARCHAR(MAX) NOT NULL
)

Go

CREATE TABLE LanguagePredictions (
    ID INT PRIMARY KEY IDENTITY,
    Response NVARCHAR(MAX) NOT NULL,
    Probability FLOAT,
    Score NVARCHAR(MAX) -- Store as JSON string
);


Go

CREATE TABLE GeneralInformation (
    ID INT PRIMARY KEY IDENTITY,
    Topic NVARCHAR(255),
    Information NVARCHAR(MAX)
)

Go

INSERT INTO GeneralInformation (Topic, Information) VALUES
('apple', 'An apple is a sweet, edible fruit produced by an apple tree.'),
('banana', 'A banana is an elongated, edible fruit produced by several kinds of large herbaceous flowering plants.'),
('cat', 'A cat is a small domesticated carnivorous mammal with soft fur, a short snout, and retractile claws.'),
('dog', 'A dog is a domesticated carnivorous mammal that typically has a long snout, an acute sense of smell, non-retractile claws, and a barking, howling, or whining voice.'),
('elephant', 'An elephant is a large mammal of the family Elephantidae and the order Proboscidea.'),
('rose', 'A rose is a woody perennial flowering plant of the genus Rosa, in the family Rosaceae.'),
('sun', 'The Sun is the star at the center of the Solar System.'),
('moon', 'The Moon is Earth''s only natural satellite.'),
('ocean', 'The ocean is a body of saline water that composes much of a planet''s hydrosphere.'),
('computer', 'A computer is a machine that can be instructed to carry out sequences of arithmetic or logical operations automatically via computer programming.')


select * from ModelFiles

select * from SentimentData

select * from SentimentPredictions

select * from LanguageData

select * from LanguagePredictions

select * from GeneralInformation









*/
