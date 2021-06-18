using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PDFViewCtrlDemo_Windows10.ViewModels.Common;
using Windows.UI.Xaml.Input;
using pdftron.PDF;

namespace PDFViewCtrlDemo_Windows10.ViewModels
{
    public class PasswordViewModel : ViewModelBase
    {
        public delegate void PasswordFinishedHandler(PDFDoc doc, bool success);
        public event PasswordFinishedHandler PasswordFinished;

        private void RaisePasswordFinished(PDFDoc doc, bool success)
        {
            if (PasswordFinished != null)
            {
                PasswordFinished(doc, success);
            }
        }

        public PasswordViewModel(PDFDoc doc)
        {
            _Doc = doc;
            InitCommands();
        }

        #region Properties

        private PDFDoc _Doc;
        public PDFDoc Doc
        {
            get { return _Doc; }
        }

        private string _CurrentPassword = string.Empty;
        public string CurrentPassword
        {
            get { return _CurrentPassword; }
            set
            {
                if (Set(ref _CurrentPassword, value))
                {
                    RaisePropertyChanged("HasPasswordBoxGotContent");
                    IsIncorrectPasswordNotificationVisible = false;
                }
            }
        }

        private bool _IsIncorrectPasswordNotificationVisible = false;
        public bool IsIncorrectPasswordNotificationVisible
        {
            get { return _IsIncorrectPasswordNotificationVisible; }
            set
            {
                Set(ref _IsIncorrectPasswordNotificationVisible, value);
                RaisePropertyChanged("HasPasswordBoxGotContent");
            }
        }

        public bool HasPasswordBoxGotContent
        {
            get { return !string.IsNullOrEmpty(CurrentPassword) && CurrentPassword.Length > 0 && IsIncorrectPasswordNotificationVisible == false; }
        }

        #endregion Properties


        #region Commands

        private void InitCommands()
        {
            PasswordKeyUpCommand = new RelayCommand(PasswordKeyUpCommandImpl);
            PasswordChangedCommand = new RelayCommand(PasswordChangedCommandImpl);
            PasswordOkPressedCommand = new RelayCommand(PasswordOkPressedCommandImpl);
            PasswordCancelPressedCommand = new RelayCommand(PasswordCancelPressedCommandImpl);
        }

        public RelayCommand PasswordKeyUpCommand { get; private set; }
        public RelayCommand PasswordChangedCommand { get; private set; }
        public RelayCommand PasswordOkPressedCommand { get; private set; }
        public RelayCommand PasswordCancelPressedCommand { get; private set; }


        private void PasswordKeyUpCommandImpl(object keyArgs)
        {
            KeyRoutedEventArgs args = keyArgs as KeyRoutedEventArgs;
            if (args != null)
            {
                PasswordKeyUp(args.Key);
            }
        }

        private void PasswordChangedCommandImpl(object newPassword)
        {
            string password = newPassword as string;
            if (password != null)
            {
                PasswordChanged(password);
            }
        }

        private void PasswordOkPressedCommandImpl(object sender)
        {
            VerifyPassword();
        }

        private void PasswordCancelPressedCommandImpl(object sender)
        {
            CancelPassword();
        }

        #endregion Commands



        private void PasswordChanged(string newPassword)
        {
            if (newPassword != null)
            {
                CurrentPassword = newPassword;
            }
        }

        private void PasswordKeyUp(Windows.System.VirtualKey key)
        {
            if (key == Windows.System.VirtualKey.Enter)
            {
                VerifyPassword();
            }
            else if (key == Windows.System.VirtualKey.Escape)
            {
                CancelPassword();
            }
        }

        private void VerifyPassword()
        {
            if (string.IsNullOrEmpty(CurrentPassword))
            {
                return;
            }
            if (_Doc.InitStdSecurityHandler(CurrentPassword))
            {
                RaisePasswordFinished(_Doc, true);
            }
            else
            {
                IsIncorrectPasswordNotificationVisible = true;
            }
        }

        private void CancelPassword()
        {
            RaisePasswordFinished(_Doc, false);
        }
    }
}
