// Authorship note:
// This interface was written by the author to support loading and saving user preferences in NutriAI.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NutriAI.Models;

namespace NutriAI.Services.Storage;

// This interface defines the basic storage operations
// for loading and saving the user's preferences.
public interface IUserPreferencesStore
{
    Task<UserPreferences> LoadAsync();
    Task SaveAsync(UserPreferences preferences);
}

