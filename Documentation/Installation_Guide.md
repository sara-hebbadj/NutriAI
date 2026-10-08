# NutriAI Installation Guide

Author: Sara Hebbadj  
Programme: MSc Artificial Intelligence  
University: University of Hull  

---

# 1. Overview

NutriAI is a cross-platform mobile application that provides personalized recipe recommendations based on user behaviour. The system tracks user interactions (views, saves, cooking actions) and applies a deterministic scoring algorithm to rank recipes according to user preferences and contextual factors.

This guide explains how to install, build, and run the NutriAI application.

---

# 2. System Requirements

The following software is required to run the NutriAI application:

- Visual Studio 2022 (Version 17.10 or newer)
- .NET 9 SDK
- .NET MAUI workload
- Android Emulator (Android Studio or Visual Studio built-in emulator)
- Internet connection (required for Spoonacular API requests)

---

# 3. Supported Platforms

The application targets the following frameworks:


net9.0-android
net9.0-ios


The Android version is used for testing and demonstration during development.

---

# 4. Project Structure

The NutriAI project follows a layered architecture based on the MVVM pattern.


NutriAI/
│
├── DTOs
│ API response objects
│
├── Models
│ Domain models used throughout the system
│
├── Preprocessing
│ Feature vector construction and normalization
│
├── Services
│ API communication, caching, and recommendation services
│
├── ViewModels
│ Application logic used by the user interface
│
├── Views
│ .NET MAUI user interface pages
│
├── Platforms
│ Platform-specific implementations


---

# 5. Installation Steps

## Step 1 — Install Visual Studio

Download and install **Visual Studio 2022** from:

https://visualstudio.microsoft.com

During installation enable the following workload:


.NET Multi-platform App UI (.NET MAUI)


---

## Step 2 — Install .NET 9 SDK

Download the .NET 9 SDK from:

https://dotnet.microsoft.com

Verify installation using:


dotnet --version


---

## Step 3 — Download the NutriAI Source Code

Extract the provided NutriAI project archive.

The root directory contains the solution file:


NutriAI.sln


---

## Step 4 — Open the Project

1. Launch **Visual Studio 2022**
2. Select **Open a Project or Solution**
3. Navigate to the extracted folder
4. Open:


NutriAI.sln


Visual Studio will automatically restore NuGet packages.

---

## Step 5 — Configure the Android Emulator

1. Open **Android Device Manager**
2. Create or start an Android emulator
3. Select the emulator as the deployment target

Example device:


Pixel 6 – Android 14


---

## Step 6 — Run the Application

In Visual Studio:

1. Select the Android emulator
2. Click **Run (▶)**
3. The application will build and launch automatically

---

# 6. External API Dependency

NutriAI retrieves recipes from the **Spoonacular Recipe API**.

API documentation:

https://spoonacular.com/food-api

The API key is no longer stored in the source code (portfolio update, October 2026).
The app reads it from the `SPOONACULAR_API_KEY` environment variable
(see `Services/SpoonacularSettings.cs`). Set it up as described in the root `README.md`,
section "How to run": on Windows with `setx SPOONACULAR_API_KEY "your-key"`, and for the
Android emulator in the git-ignored file `Platforms/Android/spoonacular.env`.


If the API quota is exceeded, requests may temporarily fail.

---

# 7. Local Data Storage

NutriAI stores user data locally using SQLite.

Stored data includes:

- user interaction history
- recipe view counts
- saved recipes
- cooking events

The database file is created automatically when the application first runs.

---

# 8. Caching

To improve performance and reduce API usage, the application uses a local caching mechanism.

Cached data includes:

- recipe lists
- recipe details

Cache durations:

| Data Type | Cache Duration |
|-----------|---------------|
Recipe search results | 12 hours |
Recipe details | 7 days |

---

# 9. Recommendation System

The recommendation pipeline performs the following steps:

1. Retrieve candidate recipes from the API
2. Apply search and filter constraints
3. Generate normalized feature vectors
4. Compute recommendation scores
5. Rank recipes according to behavioral interaction signals

Interaction signals include:

- recipe views
- saved recipes
- cooking actions

These signals are used to infer user preferences without requiring explicit ratings.

---

# 10. Troubleshooting

## Application fails to build

Ensure the following are installed:

- .NET 9 SDK
- .NET MAUI workload
- Android SDK

---

## Android emulator does not start

Restart the Android emulator or create a new virtual device in the Android Device Manager.

---

## API requests fail

Possible causes:

- expired API key
- exceeded API quota
- network connection issues

---

# 11. Contact

For questions regarding the NutriAI system implementation, please refer to the dissertation documentation.


Sara Hebbadj
MSc Artificial Intelligence
University of Hull