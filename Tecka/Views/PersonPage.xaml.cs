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
    public sealed partial class PersonPage : Page
    {
        public PersonViewModel ViewModel
        {
            get { return (PersonViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(PersonViewModel), typeof(PersonPage), new PropertyMetadata(null));

        public PersonPage()
        {
            this.InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            CertificateViewModel vm = e.ClickedItem as CertificateViewModel;
            if (vm != null)
            {
                Frame.Navigate(typeof(CertificateQrPage), new int[] { this.ViewModel.ID, vm.ID }, new SuppressNavigationTransitionInfo());
            }
        }

        private async void DeletePersonButton_Click(object sender, RoutedEventArgs e)
        {
            if (await MessageBox.ConfirmationDialogAsync("Do you really want to delete the person?", "Yes", "No", "Cancel") == true)
            {
                await ((App)App.Current).ViewModel.DeletePersonAsync(ViewModel);
                Frame.Navigate(typeof(MainPage), null, new SuppressNavigationTransitionInfo());
            }
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

            int param = (int)e.Parameter;
            var person = ((App)App.Current).ViewModel.Persons.FirstOrDefault(x => x.ID == param);
            ViewModel = person;
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
