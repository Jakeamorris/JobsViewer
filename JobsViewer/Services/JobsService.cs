using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using GalaSoft.MvvmLight.Messaging;
using JobsViewer.Model;
using JobsViewer.Services;
using JobsViewer.Entity;
using System.Data.SqlClient;

namespace JobsViewer.Service
{
    class JobsService
    {
        private static object _lock = new object();

        private ObservableCollection<JobWrapper> jobsList;
        private JobsContext db;

        public JobsService()
        {
            jobsList = new ObservableCollection<JobWrapper>();
            BindingOperations.EnableCollectionSynchronization(jobsList, _lock);

            db = new JobsContext();
        }

        public async Task GetJobsAsync()
        {
            await Task.Delay(2000);
            await Task.Run(() => GetJobs());
        }

        public async Task SaveChangesAsync()
        {
            await Task.Run(() => SaveToDatabase());
        }

        private void GetJobs()
        {
           try
            {
                List<Job> jobs = (from job
                                 in db.Jobs
                                 select job).ToList();

                jobsList.Clear();

                foreach (Job job in jobs)
                    jobsList.Add(new JobWrapper(job));

                Console.WriteLine("All jobs added");

                Messenger.Default.Send(new NotificationMessage("JobsLoaded"));
            }
            catch (SqlException ex) {
                SendError("LoadError", ex);
            }
        }

        private void SaveToDatabase()
        {
            string message;

            try
            {
                foreach (JobWrapper job in jobsList.Where(j => j.IsModified))
                {
                    if (db.Jobs.Any(j => j.Id == job.Id))
                    {
                        Job result = db.Jobs.SingleOrDefault(j => j.Id == job.Id);
                        result.JobDetails = job.JobDetails;
                        result.JobDate = job.JobDate;
                    }
                    else
                        db.Jobs.Add(job.Job);
                }

                db.SaveChanges();
                message = "SaveSuccess";

                Messenger.Default.Send(new NotificationMessage(message));

                GetJobsAsync();
            }
            catch (SqlException ex) {
                SendError("SaveError", ex);
            }
        }

        public void AddNewJob()
        {
            JobWrapper tempJob = new JobWrapper()
            {
                Id = jobsList.Count + 1,
                JobDate = DateTime.Today,
                IsModified = true
            };

            jobsList.Add(tempJob);
        }

        private void SendError(string Name, SqlException ex)
        {
            ErrorDetailsMessage errorMessage = new ErrorDetailsMessage()
            {
                ErrorName = Name,
                SqlException = ex
            };

            Messenger.Default.Send(errorMessage);
        }

        public ObservableCollection<JobWrapper> JobsList
        {
            get { return jobsList; }
        }
    }
}
