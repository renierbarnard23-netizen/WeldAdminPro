using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class AddHoldPointViewModel
        : ObservableObject
    {
        public ObservableCollection<HoldPointType>
            HoldPointTypes
        { get; }
                = new(Enum.GetValues<HoldPointType>());

        [ObservableProperty]
        private HoldPointType selectedType;

        [ObservableProperty]
        private bool isMandatory = true;

        [ObservableProperty]
        private string comments = "";

        public WeldHoldPoint? Result
        {
            get;
            private set;
        }

        private readonly Guid _weldId;

        public AddHoldPointViewModel(
            Guid weldId)
        {
            _weldId = weldId;

            SelectedType =
                HoldPointType.VisualInspection;
        }

        [RelayCommand]
        private void Save()
        {
            Result =
                new WeldHoldPoint
                {
                    Id = Guid.NewGuid(),

                    WeldId = _weldId,

                    HoldPointType = SelectedType,

                    Status =
                        HoldPointStatus.Pending,

                    IsMandatory = IsMandatory,

                    Comments = Comments
                };

            Application.Current.Windows
                .OfType<Window>()
                .SingleOrDefault(x =>
                    x.DataContext == this)
                ?.Close();
        }
    }
}