using System;
using System.Windows.Input;

namespace WeldAdminPro.UI.Helpers
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

// =====================================================================
// Integration notes (do not duplicate into source files):
// 1) Create an implementation of IUserRepository in WeldAdminPro.Data that uses your EF DbContext.
//    e.g. Task<User?> GetByUsernameAsync(string username) => _context.Users.Where(u=>u.Username==username).Select(...).FirstOrDefaultAsync();
// 2) Before showing LoginWindow, set its DataContext.LoginViewModel.UserRepo = new YourRepo();
//    Example (in App_OnStartup):
//        var login = new LoginWindow();
//        var vm = (LoginViewModel)login.DataContext;
//        vm.UserRepo = new UserRepository(yourDbContext);
//        vm.OnLoginSucceeded += (s,e) => { login.DialogResult = true; login.Close(); };
//        var res = login.ShowDialog();
//        if (res == true) { var main = new MainWindow(); main.Show(); }
// 3) Remove any plaintext password variables and prefer SecureString. Here we temporarily convert to string for PBKDF2; keep scope minimal.
// =====================================================================