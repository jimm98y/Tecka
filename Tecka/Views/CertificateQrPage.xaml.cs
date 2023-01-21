using System;
using System.Linq;
using Tecka.Utility;
using Tecka.ViewModels;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Tecka.Views
{
    public sealed partial class CertificateQrPage : Page
    {
        private int[] _navigationParameter;

        private const double MAX_QR_SIZE = 640;

        public CertificateViewModel ViewModel
        {
            get { return (CertificateViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(CertificateViewModel), typeof(CertificateQrPage), new PropertyMetadata(null));

        public CertificateQrPage()
        {
            this.InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack(new SuppressNavigationTransitionInfo());
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CertificateQrDetailPage), _navigationParameter, new SuppressNavigationTransitionInfo());
        }

        private async void DeleteCertificateButton_Click(object sender, RoutedEventArgs e)
        {
            if (await MessageBox.ConfirmationDialogAsync("Do you really want to delete the certificate?", "Yes", "No", "Cancel") == true)
            {
                await ((App)App.Current).ViewModel.DeleteCertificateAsync(ViewModel);
                Frame.Navigate(typeof(MainPage), null, new SuppressNavigationTransitionInfo());
            }
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            qrCode.MaxWidth = Math.Min(e.NewSize.Width, MAX_QR_SIZE);
            qrCode.MaxHeight = Math.Min(e.NewSize.Height, MAX_QR_SIZE);
        }

        #region Back button support on Windows 10 Mobile (navigation view does not support it natively)

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (PlatformDetectionUtils.IsWindowsMobile())
            {
                HardwareButtons.BackPressed += OnHardwareButtonPressed;
            }

            int[] param = (int[])e.Parameter;
            _navigationParameter = param;
            var certificate = ((App)App.Current).ViewModel.Persons.FirstOrDefault(x => x.ID == param[0]).Certificates.FirstOrDefault(x => x.ID == param[1]);
            ViewModel = certificate;
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
    }
}
