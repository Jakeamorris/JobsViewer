using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MvvmDialogs;
using System;
using System.Windows.Input;

namespace JobsViewer.ViewModels
{
    class ErrorDialogViewModel : ViewModelBase, IModalDialogViewModel
    {
        private bool? dialogResult;
        private Exception errorException;

        public ErrorDialogViewModel(string errorType, Exception exception)
        {
            RetryCommand = new RelayCommand(Retry);
            errorException = exception;

            GetExceptionMessage(errorType);
        }

        private void Retry()
        {
            DialogResult = true;
        }

        private void GetExceptionMessage(string type)
        {
            switch (type)
            {
                case "SaveError":
                    ErrorMessage = "An error has occured, changes could not be saved.";
                    break;

                case "LoadError":
                    ErrorMessage = "An error has occured, the database could not be loaded.";
                    break;
                default:
                    ErrorMessage = errorException.Message;
                    break;
            }
        }

        public ICommand RetryCommand { get; }

        public string ErrorMessage { get; private set; }

        public bool? DialogResult
        {
            get { return dialogResult; }
            private set => Set(nameof(DialogResult), ref dialogResult, value);
        }
    }
}
