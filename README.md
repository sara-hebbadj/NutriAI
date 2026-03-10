NutriAI – Behaviour Driven Recipe Recommendation System

Author: Sara Hebbadj
Programme: MSc Artificial Intelligence
University: University of Hull

Overview
NutriAI is a cross-platform mobile application designed to generate personalised
recipe recommendations based on user behaviour.

The system observes implicit interaction signals such as:

• Viewing recipes
• Saving recipes
• Cooking recipes

These interactions are used to adjust recommendation scores and produce
personalised results.

Technologies Used

.NET MAUI
C#
MVVM Architecture
SQLite local database
Spoonacular API

Project Structure

Views/
User interface pages

ViewModels/
Application state and UI logic

Models/
Core domain models

Services/
API communication and recommendation logic

DTOs/
API response objects

Preprocessing/
Feature processing and recommendation scoring

Installation Instructions

1. Install Visual Studio 2022
2. Install the .NET MAUI workload
3. Clone or extract this repository
4. Open NutriAI.sln
5. Restore NuGet packages
6. Run the application using an Android emulator

Supported Platforms

Android
iOS
