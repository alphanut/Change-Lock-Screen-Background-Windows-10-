using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ChangeLockScreen
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
            //Message Box.

            ShowAskPermisionMessage("Want to change the lock screen background Image?");

        }

        public async void ShowAskPermisionMessage(string message)
        {
            //Message Box.
            MessageDialog msg = new MessageDialog(message);

            //Commands
            msg.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(CommandHandlersLoock)));
            msg.Commands.Add(new UICommand("No", new UICommandInvokedHandler(CommandHandlersLoock)));

            await msg.ShowAsync();
            //end.
        }

        public async void CommandHandlersLoock(IUICommand commandLabel)
        {
            bool success = false;
            var Actions = commandLabel.Label;
            switch (Actions)
            {
                //Okay Button.
                case "Yes":
                    success = await SetWallpaperAsync("myImage.png");
                    ShowImageChangedMessage(success);
                    break;

                default:
                    break;
            }
        }


        public async void ShowImageChangedMessage(bool success)
        {
            String message = "The app gave a false when trying to load the image";
            if (success)
                message = "The image has been updated. Please lock your screen (win + L) and see if it works";
            //Message Box.

            MessageDialog msg = new MessageDialog(message);

            //Commands
            msg.Commands.Add(new UICommand("Ok", new UICommandInvokedHandler(CommandHandlersLoock)));

            await msg.ShowAsync();
            //end.
        }
        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }


        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
        async Task<bool> SetWallpaperAsync(string localAppDataFileName)
        {
            bool success = false;
            var uri = new Uri($"ms-appx:///Assets/{localAppDataFileName}");

            //generate new file name to avoid colitions
            var newFileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(localAppDataFileName)}";

            if (UserProfilePersonalizationSettings.IsSupported())
            {
                var profileSettings = UserProfilePersonalizationSettings.Current;

                var wfile = await StorageFile.GetFileFromApplicationUriAsync(uri);

                //Copy the file to Current.LocalFolder because TrySetLockScreenImageAsync
                //Will fail if the image isn't located there 
                using (Stream readStream = await wfile.OpenStreamForReadAsync(),
                              writestream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(newFileName,
                                             CreationCollisionOption.GenerateUniqueName)
                      )
                { await readStream.CopyToAsync(writestream); }

                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(newFileName);
                success = await profileSettings.TrySetLockScreenImageAsync(file);
            }

            Debug.WriteLine(success);
            return success;
        }
    }
}
