using System;
using System.Linq;
using System.Threading.Tasks;
using Tecka.Utility;
using Tecka.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Tecka.Views
{
    public sealed partial class MainPage : Page
    {
        public Tecka.ViewModels.TeckaViewModel ViewModel
        {
            get { return (Tecka.ViewModels.TeckaViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(Tecka.ViewModels.TeckaViewModel), typeof(MainPage), new PropertyMetadata(null));

        public MainPage()
        {
            ViewModel = ((App)App.Current).ViewModel;
            this.InitializeComponent();
            ExtendToTitleBar(null);
        }

        internal static void ExtendToTitleBar(UIElement titleBar)
        {
            Color background = Color.FromArgb(0xff, 0x22, 0x60, 0x9f);
            Color foreground = Colors.White;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                ApplicationViewTitleBar formattableTitleBar = ApplicationView.GetForCurrentView().TitleBar;
                formattableTitleBar.ButtonBackgroundColor = background;
                formattableTitleBar.BackgroundColor = background;
                formattableTitleBar.ButtonForegroundColor = foreground;
                formattableTitleBar.ButtonInactiveBackgroundColor = background;
                formattableTitleBar.InactiveBackgroundColor = background;
                CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.ExtendViewIntoTitleBar = false;
                //Window.Current.SetTitleBar(titleBar);
            }

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                if (statusBar != null)
                {
                    statusBar.BackgroundOpacity = 1;
                    statusBar.BackgroundColor = background;
                    statusBar.ForegroundColor = foreground;
                }
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            PersonViewModel vm = e.ClickedItem as PersonViewModel;
            if(vm != null)
            {
                Frame.Navigate(typeof(PersonPage), vm.ID, new SuppressNavigationTransitionInfo());
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddCertificatesPage), null, new SuppressNavigationTransitionInfo());
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            // Frame.Navigate(typeof(SettingsPage), null, new SuppressNavigationTransitionInfo());
        }

        #region Back button support on Windows 10 Mobile (navigation view does not support it natively)

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (PlatformDetectionUtils.IsWindowsMobile())
            {
                HardwareButtons.BackPressed += OnHardwareButtonPressed;
            }

            base.OnNavigatedTo(e);

            bool isFirstLaunch = false;

            // this is the initial page, initialize the view model
            try
            {
                isFirstLaunch = await ViewModel.InitializeAsync();
            }
            catch(Exception ex)
            {
                await MessageBox.MessageDialogAsync("Error", ex.Message, "OK");
            }

            if (isFirstLaunch)
            {
                // navigate only in case the app was just launched
                if (ViewModel.Persons.Count == 0)
                {
                    // in case there are no stored certificates, navigate directly to the add certificate page
                    Frame.Navigate(typeof(AddCertificatesPage), null, new SuppressNavigationTransitionInfo());
                }
                else if (ViewModel.Persons.Count == 1)
                {
                    // in case there is a single person, navigate directly to the add persons page
                    Frame.Navigate(typeof(PersonPage), ViewModel.Persons.Single().ID, new SuppressNavigationTransitionInfo());
                }
            }
        }

        private void OnHardwareButtonPressed(object sender, BackPressedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack(new SuppressNavigationTransitionInfo());
                e.Handled = true;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (PlatformDetectionUtils.IsWindowsMobile())
            {
                HardwareButtons.BackPressed -= OnHardwareButtonPressed;
            }

            base.OnNavigatedFrom(e);
        }

        #endregion // Back button support on Windows 10 Mobile
    }
}
