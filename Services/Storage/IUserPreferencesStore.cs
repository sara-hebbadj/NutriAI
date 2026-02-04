using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NutriAI.Models;

namespace NutriAI.Services.Storage;

public interface IUserPreferencesStore
{
    Task<UserPreferences> LoadAsync();
    Task SaveAsync(UserPreferences preferences);
}

