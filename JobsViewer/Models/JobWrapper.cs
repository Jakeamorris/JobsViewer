using JobsViewer.Entity;
using System;
using System.ComponentModel;

namespace JobsViewer.Model
{
    public class JobWrapper
    {
        private Job job;
        private bool isModified;

        public JobWrapper()
        {
            job = new Job();
        }

        public JobWrapper(Job job)
        {
            this.job = job;
        }

        public Job Job
        {
            get { return job; }
            set
            {
                if (job != value)
                    job = value;
            }
        }

        public int Id
        {
            get { return job.Id; }
            set
            {
                if (job.Id != value)
                    job.Id = value;
            }
        }

        public string JobDetails
        {
            get { return job.JobDetails; }
            set
            {
                if (job.JobDetails != value)
                {
                    job.JobDetails = value;
                    isModified = true;
                }
            }
        }

        public DateTime? JobDate
        {
            get { return job.JobDate; }
            set
            {
                if (job.JobDate != value)
                {
                    job.JobDate = value;
                    isModified = true;
                }
            }
        }

        public bool IsModified
        {
            get { return isModified; }
            set { isModified = value; }
        }
    }
}
