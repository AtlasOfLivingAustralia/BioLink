﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl {

        List<ConnectionProfile> _profiles;

        public static readonly RoutedEvent LoginSuccessfulEvent = EventManager.RegisterRoutedEvent("LoginSuccessful", RoutingStrategy.Bubble, typeof(LoginSuccessfulEventHandler), typeof(LoginControl));

        public LoginControl() {
            InitializeComponent();
            if (!this.IsDesignTime()) {
                SetupProfiles();
            }
            this.Loaded += new RoutedEventHandler(LoginControl_Loaded);
        }

        void LoginControl_Loaded(object sender, RoutedEventArgs e) {
            if (String.IsNullOrEmpty(txtUsername.Text)) {
                txtUsername.Focus();
            } else {
                txtPassword.Focus();
            }
        }

        private void SetupProfiles() {
            cmbProfile.ItemsSource = null;
            _profiles = Config.GetGlobal<List<ConnectionProfile>>("connection.profiles", new List<ConnectionProfile>());
            String lastProfile = Config.GetGlobal<string>("connection.lastprofile", null);
            if (!Config.GetGlobal<bool>("connection.skiplegacyimport", false)) {

                LegacySettings.TraverseSubKeys("Client", "UserProfiles", (key) => {
                    ConnectionProfile profile = new ConnectionProfile();
                    string name = key.Name;
                    profile.Name = key.Name.Substring(name.LastIndexOf('\\') + 1);
                    profile.Server = key.GetValue("DatabaseServer") as string;
                    profile.Database = key.GetValue("DatabaseName") as string;
                    profile.LastUser = key.GetValue("LastUser") as string;                    
                    profile.Timeout = key.GetValue("CommandTimeout") as Nullable<Int32>;
                    _profiles.Add(profile);
                });

                if (lastProfile == null) {
                    lastProfile = LegacySettings.GetRegSetting("Client", "UserProfiles", "LastUsedProfile", "");
                }

                // Save the new list
                Config.SetGlobal("connection.profiles", _profiles);
                // and we don't need to do this again!
                Config.SetGlobal("connection.skiplegacyimport", true);
            }

            cmbProfile.ItemsSource = _profiles;

            if (!String.IsNullOrEmpty(lastProfile)) {
                // Look in the list for the profile with the same name.
                ConnectionProfile lastUserProfile = _profiles.Find((item) => { return item.Name.Equals(lastProfile); });
                if (lastUserProfile != null) {
                    cmbProfile.SelectedItem = lastUserProfile;
                }
            }

        }

        public event RoutedEventHandler LoginSuccessful {
            add { AddHandler(LoginSuccessfulEvent, value); }
            remove { RemoveHandler(LoginSuccessfulEvent, value); }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e) {                        
            DoLogin();
        }

        private void DoLogin() {
            ConnectionProfile profile = cmbProfile.SelectedItem as ConnectionProfile;

            if (profile == null) {
                ErrorMessage("LoginControl.Status.SelectProfile");
                return;
            }

            Config.SetGlobal("connection.lastprofile", profile.Name);

            btnCancel.Visibility = Visibility.Hidden;
            btnLogin.Visibility = Visibility.Hidden;

            User user = new User(txtUsername.Text, txtPassword.Password, profile);

            // Save the last username...
            profile.LastUser = user.Username;
            Config.SetGlobal("connection.profiles", _profiles);
            
            string format = FindResource("LoginControl.Status.Connecting") as string;
            lblStatus.Content =  String.Format(format,  profile.Server);

            LoginAsync(user,
                () => { 
                    this.InvokeIfRequired(() => {                        
                        RaiseEvent(new LoginSuccessfulEventArgs(LoginControl.LoginSuccessfulEvent, user)); 
                    }); 
                }, 
                (errorMsg) => {
                    this.InvokeIfRequired(() => {                        
                        btnCancel.Visibility = Visibility.Visible;
                        btnLogin.Visibility = Visibility.Visible;                        
                        ErrorMessage("LoginControl.Status.LoginFailed", errorMsg);
                    });                
                }
            );
            
        }

        private void StatusMessage(string messagekey, params object[] args) {
            String message = this._R(messagekey, args);
            lblStatus.InvokeIfRequired(() => {
                lblStatus.Foreground = SystemColors.ControlTextBrush;
                lblStatus.Content = message;
            });
        }

        private void ErrorMessage(string messageKey, params object[] args) {
            string message = this._R(messageKey, args);
            lblStatus.InvokeIfRequired(() => {
                lblStatus.Foreground = new SolidColorBrush(Colors.Red);
                lblStatus.Content = message;
            });
        }


        private void LoginAsync(User user, LoginSuccessfulDelegate onSuccess, LoginFailureDelegate onFailure) {
            Thread loginThread = new Thread(new ThreadStart(() => {

                StatusMessage("LoginControl.Status.Authenticating");

                String message = "";
                if (user.Authenticate(out message)) {
                    if (onSuccess != null) {
                        onSuccess();
                    }
                } else {
                    if (onFailure != null) {
                        onFailure(message);
                    }
                }
            }));

            loginThread.IsBackground = true;
            loginThread.Start();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Environment.Exit(0);
        }

        private delegate void LoginSuccessfulDelegate();
        private delegate void LoginFailureDelegate(string message);

        private void cmbProfile_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ConnectionProfile profile = cmbProfile.SelectedItem as ConnectionProfile;
            if (profile != null) {
                txtUsername.IsEnabled = !profile.IntegratedSecurity;
                txtPassword.IsEnabled = !profile.IntegratedSecurity;
                lblUsername.IsEnabled = !profile.IntegratedSecurity;
                lblPassword.IsEnabled = !profile.IntegratedSecurity;

                txtPassword.Password = "";
                if (profile.IntegratedSecurity) {
                    txtUsername.Text = "";                    
                } else {
                    txtUsername.Text = profile.LastUser;
                }                
            }
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e) {
            ConnectionProfiles window = new ConnectionProfiles();
            window.Owner = MainWindow.Instance;
            // Show the profile manager
            if (window.ShowDialog().GetValueOrDefault(false)) {
                // if ok was pressed, reload the profiles
                SetupProfiles();
                // and select the last one selected in the profile manager
                if (window.SelectedProfile != null) {
                    SelectProfileByName(window.SelectedProfile.Name);
                }
            }
        }

        private void SelectProfileByName(string name) {
            var profiles = cmbProfile.ItemsSource as List<ConnectionProfile>;
            foreach (ConnectionProfile profile in profiles) {
                if (profile.Name.Equals(name)) {
                    cmbProfile.SelectedItem = profile;
                    break;
                }
            }            
        }

    }

    public delegate void LoginSuccessfulEventHandler(object sender, LoginSuccessfulEventArgs e);

    public class LoginSuccessfulEventArgs : RoutedEventArgs {

        public User User { get; set; }

        public LoginSuccessfulEventArgs(RoutedEvent @event, User user)
            : base(@event) {
                this.User = user;
        }
    }

}
