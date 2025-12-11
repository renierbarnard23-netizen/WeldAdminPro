using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace WeldAdminPro.UI.Services
{
    /// <summary>
    /// Simple file-backed user-state store. Stores a tiny JSON blob under %LocalAppData%\WeldAdminPro\userstate.json
    /// Best effort: swallows IO errors so it never crashes the app.
    /// </summary>
    public class UserStateService
    {
        private readonly string _path;

        public UserStateService(string? filename = null)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dir = Path.Combine(appData, "WeldAdminPro");
            try { Directory.CreateDirectory(dir); } catch { }
            _path = filename is null ? Path.Combine(dir, "userstate.json") : Path.Combine(dir, filename);
        }

        private class State
        {
            public string? SelectedProjectId { get; set; }
            public string? SelectedNavId { get; set; }
        }

        private State LoadState()
        {
            try
            {
                if (!File.Exists(_path)) return new State();
                var txt = File.ReadAllText(_path);
                if (string.IsNullOrWhiteSpace(txt)) return new State();
                return JsonSerializer.Deserialize<State>(txt) ?? new State();
            }
            catch
            {
                return new State();
            }
        }

        private void SaveState(State s)
        {
            try
            {
                var txt = JsonSerializer.Serialize(s, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_path, txt);
            }
            catch
            {
                // ignore IO errors on save
            }
        }

        public string? GetSelectedProjectId()
        {
            return LoadState().SelectedProjectId;
        }

        public void SetSelectedProjectId(string? id)
        {
            var s = LoadState();
            s.SelectedProjectId = id;
            SaveState(s);
        }

        public string? GetSelectedNavId()
        {
            return LoadState().SelectedNavId;
        }

        public void SetSelectedNavId(string? id)
        {
            var s = LoadState();
            s.SelectedNavId = id;
            SaveState(s);
        }
    }
}
