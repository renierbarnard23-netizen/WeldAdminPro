using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WeldAdminPro.UI.ViewModels
{
    /// <summary>
    /// Canonical NavItem used by left nav.
    /// Use FontFamily="Segoe MDL2 Assets" and set Glyph to a MDL2 code like "\uE700".
    /// This class exposes both Title and Label (Label kept for backwards compatibility).
    /// </summary>
    public class NavItem : ObservableObject
    {
        public NavItem() { }

        public NavItem(string title, string glyph = "", ICommand? command = null)
        {
            Id = Guid.NewGuid().ToString();
            Title = title;
            Label = title; // keep Label in sync by default
            Glyph = glyph;
            Command = command;
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Human-friendly title (preferred new name).
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Backwards-compatible Label property (some code still sets Label).
        /// Setting Label does NOT update Title automatically unless you explicitly do so.
        /// </summary>
        private string _label = string.Empty;
        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        /// <summary>
        /// MDL2 glyph (as string) or font glyph char. Use FontFamily="Segoe MDL2 Assets" in XAML.
        /// Example glyphs you can use: "\uE8A5" (Book), "\uE82D" (View), "\uE8F1" (Document), "\uE8B8" (Report).
        /// </summary>
        public string Glyph { get; set; } = string.Empty;

        public ICommand? Command { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
