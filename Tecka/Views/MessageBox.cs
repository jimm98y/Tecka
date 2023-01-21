using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Tecka.Views
{
    public static class MessageBox
    {
        public static async Task MessageDialogAsync(
            string title,
            string message,
            string buttonText)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = buttonText
            };

            await dialog.ShowAsync();
        }

        public static async Task<bool?> ConfirmationDialogAsync(
            string title,
            string yesButtonText,
            string noButtonText,
            string cancelButtonText)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = yesButtonText,
                SecondaryButtonText = noButtonText,
                CloseButtonText = cancelButtonText
            };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.None)
            {
                return null;
            }

            return (result == ContentDialogResult.Primary);
        }
    }
}
