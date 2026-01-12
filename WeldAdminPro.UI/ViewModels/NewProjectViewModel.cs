using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class NewProjectViewModel : ObservableObject
    {
        private readonly ProjectRepository _repo;

        //public NewProjectViewModel(ProjectRepository repo)
        //{
            //_repo = repo;
        //}

        [ObservableProperty]
        private string projectName;

        [RelayCommand]
        private void Save()
        {
            // Save logic
        }
    }
}
