using System.Linq;
using Tecka.Utility;
using Tecka.ViewModels;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Tecka.Views
{
    public sealed partial class CertificateQrDetailPage : Page
    {
        private readonly string _currentCulture;

        public CertificateViewModel ViewModel
        {
            get { return (CertificateViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(CertificateViewModel), typeof(CertificateQrDetailPage), new PropertyMetadata(null));

        public CertificateQrDetailPage()
        {
            this.InitializeComponent();

            _currentCulture = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            languageToggle.Content = _currentCulture == "en" ? "CS" : "EN";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack(new SuppressNavigationTransitionInfo());
        }

        #region Back button support on Windows 10 Mobile (navigation view does not support it natively)

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (PlatformDetectionUtils.IsWindowsMobile())
            {
                HardwareButtons.BackPressed += OnHardwareButtonPressed;
            }

            int[] param = (int[])e.Parameter;
            var certificate = ((App)App.Current).ViewModel.Persons.FirstOrDefault(x => x.ID == param[0]).Certificates.FirstOrDefault(x => x.ID == param[1]);
            ViewModel = certificate;
            ViewModel.ChangeLanguage(_currentCulture); // make sure we start with the default culture
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

        private void TranslateToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if(_currentCulture == "en")
            {
                ViewModel.ChangeLanguage("cs");
            }
            else
            {
                ViewModel.ChangeLanguage("en");
            }
        }

        private void TranslateToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_currentCulture == "en")
            {
                ViewModel.ChangeLanguage("en");
            }
            else
            {
                ViewModel.ChangeLanguage("cs");
            }
        }
    }
}
