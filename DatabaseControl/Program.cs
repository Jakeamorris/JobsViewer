using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;

namespace DatabaseControl
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Job> JobsList = new List<Job>();

            using (var reader = new StreamReader("../../../ref.csv"))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = ";";
                csv.Configuration.IgnoreQuotes = true;
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var job = new Job
                    {
                        Id = csv.GetField<int>("Id"),
                        JobDetails = csv.GetField("JobDetails")
                    };

                    if (!String.IsNullOrEmpty(csv.GetField("JobDate")))
                        job.JobDate = DateTime.Parse(csv.GetField("JobDate"));

                    JobsList.Add(job);
                }
            }

            using (var db = new JobsContext())
            {
                foreach (Job job in JobsList)
                {
                    db.Jobs.Add(job);
                    Console.WriteLine("Job no. " + job.Id + " added to the database.");
                }

                Console.WriteLine("Saving...");

                db.SaveChanges();
                db.Dispose();
            }
        }
    }
}
