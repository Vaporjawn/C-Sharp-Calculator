using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ModernCalculator.Services
{
    /// <summary>
    /// Represents a calculation history item
    /// </summary>
    public class HistoryItem : INotifyPropertyChanged
    {
        private string _expression = string.Empty;
        private double _result;
        private DateTime _timestamp;
        private bool _isFavorite;

        public string Expression
        {
            get => _expression;
            set
            {
                _expression = value;
                OnPropertyChanged(nameof(Expression));
            }
        }

        public double Result
        {
            get => _result;
            set
            {
                _result = value;
                OnPropertyChanged(nameof(Result));
            }
        }

        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                _timestamp = value;
                OnPropertyChanged(nameof(Timestamp));
            }
        }

        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                _isFavorite = value;
                OnPropertyChanged(nameof(IsFavorite));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Service for managing calculation history
    /// </summary>
    public interface IHistoryService
    {
        /// <summary>
        /// Gets all history items
        /// </summary>
        IEnumerable<HistoryItem> GetHistory();

        /// <summary>
        /// Gets favorite history items
        /// </summary>
        IEnumerable<HistoryItem> GetFavorites();

        /// <summary>
        /// Adds a calculation to history
        /// </summary>
        Task AddToHistoryAsync(string expression, double result);

        /// <summary>
        /// Removes an item from history
        /// </summary>
        Task RemoveFromHistoryAsync(HistoryItem item);

        /// <summary>
        /// Clears all history
        /// </summary>
        Task ClearHistoryAsync();

        /// <summary>
        /// Toggles favorite status of a history item
        /// </summary>
        Task ToggleFavoriteAsync(HistoryItem item);

        /// <summary>
        /// Searches history by expression
        /// </summary>
        IEnumerable<HistoryItem> SearchHistory(string searchTerm);

        /// <summary>
        /// Exports history to file
        /// </summary>
        Task ExportHistoryAsync(string filePath);

        /// <summary>
        /// Imports history from file
        /// </summary>
        Task ImportHistoryAsync(string filePath);

        /// <summary>
        /// Event fired when history changes
        /// </summary>
        event EventHandler<HistoryItem>? HistoryAdded;
        event EventHandler<HistoryItem>? HistoryRemoved;
        event EventHandler? HistoryCleared;
    }

    /// <summary>
    /// Theme service for managing application themes
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Gets the current theme
        /// </summary>
        string CurrentTheme { get; }

        /// <summary>
        /// Gets available themes
        /// </summary>
        IEnumerable<string> AvailableThemes { get; }

        /// <summary>
        /// Sets the application theme
        /// </summary>
        Task SetThemeAsync(string themeName);

        /// <summary>
        /// Toggles between light and dark themes
        /// </summary>
        Task ToggleThemeAsync();

        /// <summary>
        /// Event fired when theme changes
        /// </summary>
        event EventHandler<string>? ThemeChanged;
    }

    /// <summary>
    /// Settings service for application preferences
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Gets a setting value
        /// </summary>
        T GetSetting<T>(string key, T defaultValue = default!);

        /// <summary>
        /// Sets a setting value
        /// </summary>
        Task SetSettingAsync<T>(string key, T value);

        /// <summary>
        /// Removes a setting
        /// </summary>
        Task RemoveSettingAsync(string key);

        /// <summary>
        /// Checks if a setting exists
        /// </summary>
        bool HasSetting(string key);

        /// <summary>
        /// Saves all settings to storage
        /// </summary>
        Task SaveAsync();

        /// <summary>
        /// Loads settings from storage
        /// </summary>
        Task LoadAsync();

        /// <summary>
        /// Resets all settings to defaults
        /// </summary>
        Task ResetToDefaultsAsync();

        /// <summary>
        /// Event fired when a setting changes
        /// </summary>
        event EventHandler<(string Key, object? Value)>? SettingChanged;
    }
}
