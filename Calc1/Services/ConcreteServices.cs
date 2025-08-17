using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModernCalculator.Services
{
    /// <summary>
    /// Implementation of the history service
    /// </summary>
    public class HistoryService : IHistoryService
    {
        private readonly ObservableCollection<HistoryItem> _history = new();
        private readonly string _historyFilePath;
        private const int MaxHistoryItems = 1000;

        public HistoryService()
        {
            _historyFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ModernCalculator",
                "history.json"
            );

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_historyFilePath)!);

            // Load history on startup
            _ = LoadHistoryAsync();
        }

        public event EventHandler<HistoryItem>? HistoryAdded;
        public event EventHandler<HistoryItem>? HistoryRemoved;
        public event EventHandler? HistoryCleared;

        public IEnumerable<HistoryItem> GetHistory() => _history.OrderByDescending(h => h.Timestamp);

        public IEnumerable<HistoryItem> GetFavorites() => _history.Where(h => h.IsFavorite).OrderByDescending(h => h.Timestamp);

        public async Task AddToHistoryAsync(string expression, double result)
        {
            var historyItem = new HistoryItem
            {
                Expression = expression,
                Result = result,
                Timestamp = DateTime.Now
            };

            _history.Insert(0, historyItem);

            // Limit history size
            while (_history.Count > MaxHistoryItems)
            {
                _history.RemoveAt(_history.Count - 1);
            }

            HistoryAdded?.Invoke(this, historyItem);
            await SaveHistoryAsync();
        }

        public async Task RemoveFromHistoryAsync(HistoryItem item)
        {
            if (_history.Remove(item))
            {
                HistoryRemoved?.Invoke(this, item);
                await SaveHistoryAsync();
            }
        }

        public async Task ClearHistoryAsync()
        {
            _history.Clear();
            HistoryCleared?.Invoke(this, EventArgs.Empty);
            await SaveHistoryAsync();
        }

        public async Task ToggleFavoriteAsync(HistoryItem item)
        {
            item.IsFavorite = !item.IsFavorite;
            await SaveHistoryAsync();
        }

        public IEnumerable<HistoryItem> SearchHistory(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetHistory();

            return _history
                .Where(h => h.Expression.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           h.Result.ToString().Contains(searchTerm))
                .OrderByDescending(h => h.Timestamp);
        }

        public async Task ExportHistoryAsync(string filePath)
        {
            try
            {
                var historyData = _history.Select(h => new
                {
                    h.Expression,
                    h.Result,
                    h.Timestamp,
                    h.IsFavorite
                }).ToList();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(historyData, options);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to export history: {ex.Message}", ex);
            }
        }

        public async Task ImportHistoryAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("History file not found.");

                var json = await File.ReadAllTextAsync(filePath);
                var historyData = JsonSerializer.Deserialize<dynamic[]>(json);

                if (historyData != null)
                {
                    _history.Clear();
                    // Implementation would parse and add imported items
                    HistoryCleared?.Invoke(this, EventArgs.Empty);
                    await SaveHistoryAsync();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to import history: {ex.Message}", ex);
            }
        }

        private async Task SaveHistoryAsync()
        {
            try
            {
                var historyData = _history.Take(MaxHistoryItems).Select(h => new
                {
                    h.Expression,
                    h.Result,
                    h.Timestamp,
                    h.IsFavorite
                }).ToList();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(historyData, options);
                await File.WriteAllTextAsync(_historyFilePath, json);
            }
            catch (Exception ex)
            {
                // Log error but don't throw - history saving shouldn't crash the app
                System.Diagnostics.Debug.WriteLine($"Failed to save history: {ex.Message}");
            }
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                if (!File.Exists(_historyFilePath))
                    return;

                var json = await File.ReadAllTextAsync(_historyFilePath);
                using var document = JsonDocument.Parse(json);

                foreach (var element in document.RootElement.EnumerateArray())
                {
                    if (element.TryGetProperty("Expression", out var exprProp) &&
                        element.TryGetProperty("Result", out var resultProp) &&
                        element.TryGetProperty("Timestamp", out var timestampProp))
                    {
                        var historyItem = new HistoryItem
                        {
                            Expression = exprProp.GetString() ?? "",
                            Result = resultProp.GetDouble(),
                            Timestamp = timestampProp.GetDateTime(),
                            IsFavorite = element.TryGetProperty("IsFavorite", out var favProp) && favProp.GetBoolean()
                        };

                        _history.Add(historyItem);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load history: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Implementation of the theme service
    /// </summary>
    public class ThemeService : IThemeService
    {
        private string _currentTheme = "Light";
        private readonly string[] _availableThemes = { "Light", "Dark", "HighContrast", "Blue", "Green" };

        public string CurrentTheme => _currentTheme;
        public IEnumerable<string> AvailableThemes => _availableThemes;

        public event EventHandler<string>? ThemeChanged;

        public async Task SetThemeAsync(string themeName)
        {
            if (_availableThemes.Contains(themeName) && _currentTheme != themeName)
            {
                _currentTheme = themeName;
                await ApplyThemeAsync(themeName);
                ThemeChanged?.Invoke(this, themeName);
            }
        }

        public async Task ToggleThemeAsync()
        {
            var nextTheme = _currentTheme == "Light" ? "Dark" : "Light";
            await SetThemeAsync(nextTheme);
        }

        private async Task ApplyThemeAsync(string themeName)
        {
            // Apply theme resources to the application
            if (System.Windows.Application.Current?.Resources != null)
            {
                var app = System.Windows.Application.Current;

                // Clear current theme resources
                var resourcesToRemove = app.Resources.MergedDictionaries
                    .Where(rd => rd.Source?.OriginalString.Contains("Theme") == true)
                    .ToList();

                foreach (var resource in resourcesToRemove)
                {
                    app.Resources.MergedDictionaries.Remove(resource);
                }

                // Add new theme resources
                try
                {
                    var themeUri = new Uri($"pack://application:,,,/ModernCalculator;component/Resources/Themes/{themeName}Theme.xaml");
                    var themeDict = new System.Windows.ResourceDictionary { Source = themeUri };
                    app.Resources.MergedDictionaries.Add(themeDict);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load theme {themeName}: {ex.Message}");
                }
            }

            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Implementation of the settings service
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly Dictionary<string, object?> _settings = new();
        private readonly string _settingsFilePath;

        public SettingsService()
        {
            _settingsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ModernCalculator",
                "settings.json"
            );

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath)!);

            // Load settings on startup
            _ = LoadAsync();
        }

        public event EventHandler<(string Key, object? Value)>? SettingChanged;

        public T GetSetting<T>(string key, T defaultValue = default!)
        {
            if (_settings.TryGetValue(key, out var value))
            {
                try
                {
                    if (value is JsonElement jsonElement)
                    {
                        return JsonSerializer.Deserialize<T>(jsonElement.GetRawText()) ?? defaultValue;
                    }
                    return (T?)value ?? defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        public async Task SetSettingAsync<T>(string key, T value)
        {
            var oldValue = _settings.TryGetValue(key, out var existing) ? existing : null;
            _settings[key] = value;

            SettingChanged?.Invoke(this, (key, value));

            if (!Equals(oldValue, value))
            {
                await SaveAsync();
            }
        }

        public async Task RemoveSettingAsync(string key)
        {
            if (_settings.Remove(key))
            {
                SettingChanged?.Invoke(this, (key, null));
                await SaveAsync();
            }
        }

        public bool HasSetting(string key) => _settings.ContainsKey(key);

        public async Task SaveAsync()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(_settings, options);
                await File.WriteAllTextAsync(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        public async Task LoadAsync()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                    return;

                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var loadedSettings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                if (loadedSettings != null)
                {
                    _settings.Clear();
                    foreach (var kvp in loadedSettings)
                    {
                        _settings[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }
        }

        public async Task ResetToDefaultsAsync()
        {
            _settings.Clear();
            await SaveAsync();
        }
    }
}
