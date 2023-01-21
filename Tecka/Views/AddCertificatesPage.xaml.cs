using System;
using Tecka.Utility;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Tecka.Views
{
    public sealed partial class AddCertificatesPage : Page
    {
        public Tecka.ViewModels.TeckaViewModel ViewModel { get; }

        public AddCertificatesPage()
        {
            ViewModel = ((App)App.Current).ViewModel;
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack(new SuppressNavigationTransitionInfo());
        }

        private void HideSmsFlyout_Click(object sender, RoutedEventArgs e)
        {
            this.smsFlyout.Hide();
        }

        private void HideQrFlyout_Click(object sender, RoutedEventArgs e)
        {
            this.qrFlyout.Hide();
        }

        private void HideEidFlyout_Click(object sender, RoutedEventArgs e)
        {
            this.eidFlyout.Hide();
        }

        #region Back button support on Windows 10 Mobile (navigation view does not support it natively)

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (PlatformDetectionUtils.IsWindowsMobile())
            {
                HardwareButtons.BackPressed += OnHardwareButtonPressed;
            }

            base.OnNavigatedTo(e);
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

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            switch((string)((Grid)e.ClickedItem).Tag)
            {
                case "sms":
                    try
                    {
                        await ViewModel.AddPersonFromUzisAsync();
                        Frame.Navigate(typeof(MainPage), null, new SuppressNavigationTransitionInfo());
                    }
                    catch(Exception ex)
                    {
                        // TODO error
                    }
                    break;

                case "qr":
                    Frame.Navigate(typeof(ScanQRCodePage), null, new SuppressNavigationTransitionInfo());
                    break;

                case "eid":
                    break;
            }
        }
    }
}
