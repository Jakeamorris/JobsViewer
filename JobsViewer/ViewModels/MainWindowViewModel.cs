using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MvvmDialogs;
using JobsViewer.Model;
using JobsViewer.Service;
using JobsViewer.Services;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace JobsViewer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IDialogService dialogService;

        private JobsService jobsService;
        private GoogleDriveService driveService;
        
        private ICollectionView jobsCollectionView;

        private bool searchVisibility;
        private bool spinnerVisibility;
        private bool notificationVisibility;
        private bool largeSpinnerVisibility;
        private bool saveButtonIsEnabled;
        private string notificationContent;
        private int fontSize;

        private int searchJobNumber;
        private string searchJobDetails;
        private DateTime searchJobDateFrom;
        private DateTime searchJobDateTo;

        public MainWindowViewModel(IDialogService dialogService)
        {
            MessengerInstance.Register<NotificationMessage>(this, MessageHandler);
            MessengerInstance.Register<ErrorDetailsMessage>(this, ErrorHandler);

            jobsService = new JobsService();
            //driveService = new GoogleDriveService();

            jobsCollectionView = CollectionViewSource.GetDefaultView(jobsService.JobsList);
            jobsCollectionView.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));

            this.dialogService = dialogService;
            searchVisibility = true;
            largeSpinnerVisibility = true;
            spinnerVisibility = false;
            notificationVisibility = false;
            saveButtonIsEnabled = false;
            fontSize = Properties.Settings.Default.FontSize;
            searchJobDateFrom = DateTime.Parse("01/01/2000");
            searchJobDateTo = DateTime.Today;

            InitialiseCommands();
        }

        private void InitialiseCommands()
        {
            LoadedCommand = new RelayCommand(() => LoadedFunction());
            NewJobCommand = new RelayCommand(() => NewJobFunction());
            SaveCommand = new RelayCommand(() => SaveFunction());
            RefreshCommand = new RelayCommand(() => RefreshFunction());
            ShowSearchCommand = new RelayCommand(() => ShowSearchFunction());
            SearchJobsCommand = new RelayCommand(() => SearchJobsFunction());
            ClearSearchCommand = new RelayCommand(() => ClearSearchFunction());
            CloseCommand = new RelayCommand(() => OnCloseFunction());
        }

        private void MessageHandler(NotificationMessage message)
        {
            switch (message.Notification)
            {
                case "JobsLoaded":
                    LargeSpinnerVisibility = false;
                    SaveButtonIsEnabled = true;
                    NotificationOut("Database Loaded.");
                    break;
                case "SaveSuccess":
                    NotificationOut("Changes Saved.");
                    SaveButtonIsEnabled = true;
                    break;
            }
        }

        private void ErrorHandler(ErrorDetailsMessage message)
        {
            NotificationOut("Error");

            var dialogViewModel = new ErrorDialogViewModel(message.ErrorName, message.SqlException);
            bool? retry = false;

            //Application.Current.Dispatcher.Invoke((Action)delegate {
                retry = dialogService.ShowDialog(this, dialogViewModel);
            //});

            if (retry == true)
            {
                switch (message.ErrorName)
                {
                    case "SaveError":
                        SaveFunction();
                        break;

                    case "LoadError":
                        jobsService.GetJobsAsync();
                        break;
                }
            }

            SaveButtonIsEnabled = true;
        }

        private void RefreshJobsList()
        {
            RaisePropertyChanged(() => Jobs);
        }

        private void NotificationIn(string message)
        {
            SpinnerVisibility = true;
            NotificationVisibility = true;
            NotificationContent = message;
        }

        private void NotificationOut(string message)
        {
            SpinnerVisibility = false;
            NotificationVisibility = false;
            NotificationContent = message;
        }

        //Commands
        public RelayCommand LoadedCommand { get; set; }

        public RelayCommand NewJobCommand { get; set; }

        public RelayCommand SaveCommand { get; set; }

        public RelayCommand RefreshCommand { get; set; }

        public RelayCommand ShowSearchCommand { get; set; }

        public RelayCommand CloseCommand { get; set; }

        public RelayCommand SearchJobsCommand { get; set; }

        public RelayCommand ClearSearchCommand { get; set; }

        private void LoadedFunction()
        {
            NotificationIn("Loading...");
            jobsService.GetJobsAsync();
        }

        private void RefreshFunction()
        {
            NotificationIn("Loading...");
            jobsService.GetJobsAsync();
        }

        private void NewJobFunction()
        {
            jobsService.AddNewJob();
            RaisePropertyChanged(() => Jobs);
        }

        private void SaveFunction()
        {
            NotificationIn("Saving...");
            SaveButtonIsEnabled = false;
            jobsService.SaveChangesAsync();
        }

        private void ShowSearchFunction()
        {
            SearchVisibility = !SearchVisibility;
        }

        private void OnCloseFunction()
        {
            Properties.Settings.Default.Save();
        }

        private void SearchJobsFunction()
        {
            if (searchJobDateFrom.Date >= searchJobDateTo.Date)
            {
                DateTime tempDate = searchJobDateFrom;

                searchJobDateFrom = searchJobDateTo;
                searchJobDateTo = tempDate;
            }

            jobsCollectionView.Filter = new Predicate<object>(SearchFilter);
        }

        private bool SearchFilter(object ob)
        {
            JobWrapper job = ob as JobWrapper;

            if (searchJobNumber != 0)
                if (job.Id != searchJobNumber)
                    return false;

            if (!String.IsNullOrEmpty(searchJobDetails))
                if (job.JobDetails.IndexOf(searchJobDetails, StringComparison.OrdinalIgnoreCase) == -1)
                    return false;

            if (job.JobDate?.Date < searchJobDateFrom.Date || job.JobDate?.Date > searchJobDateTo.Date)
                return false;

            return true;
        }

        private void ClearSearchFunction()
        {
            jobsCollectionView.Filter = null;

            SearchJobNumber = null;
            SearchJobDetails = null;
            SearchJobDateFrom = DateTime.Parse("01/01/2000");
            SearchJobDateTo = DateTime.Today;
        }

        //Getters & Setters
        public ICollectionView Jobs
        {
            get { return jobsCollectionView; }
        }

        public bool SearchVisibility
        {
            get { return searchVisibility; }
            set
            {
                if (searchVisibility != value)
                {
                    searchVisibility = value;
                    RaisePropertyChanged(() => SearchVisibility);
                }
            }
        }

        public bool SpinnerVisibility
        {
            get { return spinnerVisibility; }
            set
            {
                if (spinnerVisibility != value)
                {
                    spinnerVisibility = value;
                    RaisePropertyChanged(() => SpinnerVisibility);
                }
            }
        }

        public bool LargeSpinnerVisibility
        {
            get { return largeSpinnerVisibility; }
            set
            {
                if (largeSpinnerVisibility != value)
                {
                    largeSpinnerVisibility = value;
                    RaisePropertyChanged(() => LargeSpinnerVisibility);
                }
            }
        }

        public bool NotificationVisibility
        {
            get { return notificationVisibility; }
            set
            {
                if (notificationVisibility != value)
                {
                    notificationVisibility = value;
                    RaisePropertyChanged(() => NotificationVisibility);
                }
            }
        }

        public bool SaveButtonIsEnabled
        {
            get { return saveButtonIsEnabled; }
            set
            {
                if (saveButtonIsEnabled != value)
                {
                    saveButtonIsEnabled = value;
                    RaisePropertyChanged(() => SaveButtonIsEnabled);
                }
            }
        }

        public string NotificationContent
        {
            get { return notificationContent; }
            set
            {
                if (notificationContent != value)
                {
                    notificationContent = value;
                    RaisePropertyChanged(() => NotificationContent);
                }
            }
        }

        public int FontSize
        {
            get { return fontSize; }
            set
            {
                if (fontSize != value)
                {
                    fontSize = value;
                    Properties.Settings.Default.FontSize = fontSize;
                    RaisePropertyChanged(() => FontSize);
                }
            }
        }

        public string SearchJobNumber
        {
            get
            {
                if (searchJobNumber == 0)
                    return String.Empty;
                else
                    return searchJobNumber.ToString();
            }
            set
            {
                if (!Int32.TryParse(value, out searchJobNumber))
                    searchJobNumber = 0;

                RaisePropertyChanged(() => SearchJobNumber);
            }
        }

        public string SearchJobDetails
        {
            get { return searchJobDetails; }
            set
            {
                if (searchJobDetails != value)
                {
                    searchJobDetails = value;
                    RaisePropertyChanged(() => SearchJobDetails);
                }
            }
        }

        public DateTime SearchJobDateFrom
        {
            get { return searchJobDateFrom; }
            set
            {
                if (searchJobDateFrom != value)
                {
                    searchJobDateFrom = value;
                    RaisePropertyChanged(() => SearchJobDateFrom);
                }
            }
        }

        public DateTime SearchJobDateTo
        {
            get { return searchJobDateTo; }
            set
            {
                if (searchJobDateTo != value)
                {
                    searchJobDateTo = value;
                    RaisePropertyChanged(() => SearchJobDateTo);
                }
            }
        }
    }
}