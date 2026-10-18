using System;
using System.Collections.Generic;
using ObservableCollections;
using R3;

namespace NetNotepad.Client.Source
{
    public static class DataManager
    {
        public static IObservableCollection<KeyValuePair<UserID, UserProfile>> UserList => _userList;
        public static ISynchronizedView<KeyValuePair<string, int>, string> HostList { get; }

        private static ObservableDictionary<UserID, UserProfile> _userList = [];
        private static ObservableDictionary<string, int> _hostList = [];

        static DataManager()
        {
            HostList = _hostList.CreateView(e => e.Key);
            _userList.ObserveAdd().Subscribe(e =>
            {
                if (!_hostList.TryGetValue(e.Value.Key.UserServer, out int i)) { _hostList.Add(e.Value.Key.UserServer, 1); }
                else { _hostList[e.Value.Key.UserServer] = i + 1; }
            });
            _userList.ObserveRemove().Subscribe(e =>
            {
                if (_hostList.TryGetValue(e.Value.Key.UserServer, out int i))
                {
                    if (i > 0) { _hostList[e.Value.Key.UserServer] = i - 1; }
                    else { _hostList.Remove(e.Value.Key.UserServer); }
                }
            });
        }

        public static void LoadUsers(params UserProfile[] profiles) { foreach (UserProfile profile in profiles) { AddUser(profile); } }
        public static void AddUser(UserProfile profile) { if (ValidateProfile(profile)) { _userList.Add(profile.UserID, profile); } }
        public static void RemoveUser(UserID userID) { if (ValidateProfile(userID)) { _userList.Remove(userID); } }

        private static bool ValidateProfile(UserProfile profile) => ValidateProfile(profile.UserID) &&
            !string.IsNullOrEmpty(profile.UserLogin) &&
            !string.IsNullOrEmpty(profile.RefreshTokenKey);
        private static bool ValidateProfile(UserID id) => id.UserGuid != Guid.Empty && !string.IsNullOrEmpty(id.UserServer);

        public record UserProfile(UserID UserID, string UserLogin, DateTime LastLogin, string RefreshTokenKey, DateTime PasswordDate);
        public record UserID(Guid UserGuid, string UserServer);
    }
}