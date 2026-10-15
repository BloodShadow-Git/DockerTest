using System;
using System.Collections.Generic;
using ObservableCollections;

namespace NetNotepad.Client.Source
{
    public static class DataManager
    {
        public static IObservableCollection<KeyValuePair<UserID, UserProfile>> UserList => _userList;

        private static ObservableDictionary<UserID, UserProfile> _userList = [];

        public static void LoadUsers(params UserProfile[] profiles) { foreach (UserProfile profile in profiles) { AddUser(profile); } }
        public static void AddUser(UserProfile profile) { if (ValidateProfile(profile)) { _userList.Add(profile.UserID, profile); } }
        public static void RemoveUser(UserID userID) { if (ValidateProfile(userID)) { _userList.Remove(userID); } }

        private static bool ValidateProfile(UserProfile profile) => ValidateProfile(profile.UserID) ||
            !string.IsNullOrEmpty(profile.UserLogin) ||
            !string.IsNullOrEmpty(profile.RefreshTokenKey);
        private static bool ValidateProfile(UserID id) => id.UserGuid != Guid.Empty || !string.IsNullOrEmpty(id.UserServer);

        public record UserProfile(UserID UserID, string UserLogin, DateTime LastLogin, string RefreshTokenKey, DateTime PasswordDate);
        public record UserID(Guid UserGuid, string UserServer);
    }
}