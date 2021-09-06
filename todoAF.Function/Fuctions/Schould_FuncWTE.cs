using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;
using WorkTimeEmp.Function.Entities;

namespace WorkTimeEmp.Function.Fuctions
{
    public static class Schould_FuncWTE
    {
        [FunctionName(nameof(WorkTime))]
        public static async Task WorkTime(
            [TimerTrigger("0 */1 * * * *")] TimerInfo myTimer,
            [Table("WorkTimeEmpEntity", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            [Table("ConsolEntity", Connection = "AzureWebJobsStorage")] CloudTable timeTable2,
            ILogger log)
        {
            //Table 1
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<WorkTimeEmpEntity> query = new TableQuery<WorkTimeEmpEntity>().Where(filter);
            TableQuerySegment<WorkTimeEmpEntity> allCheckEntity = await timeTable.ExecuteQuerySegmentedAsync(query, null);

            //Table 2
            TableQuery<ConsolEntity> QueryConsolidate = new TableQuery<ConsolEntity>().Where(filter);
            TableQuerySegment<ConsolEntity> ConEntity = await timeTable2.ExecuteQuerySegmentedAsync(QueryConsolidate, null);

            log.LogInformation($"First cicle");
            foreach (WorkTimeEmpEntity iterator in allCheckEntity)
            {
                log.LogInformation($"First if");
                if (!string.IsNullOrEmpty(iterator.Idemployee.ToString()) && iterator.Type == 0)
                {
                    log.LogInformation($"Second foreach");
                    foreach (WorkTimeEmpEntity iteratortwo in allCheckEntity)
                    {
                        TimeSpan dateCalculated = (iteratortwo.WorkingHour - iterator.WorkingHour);
                        //log.LogInformation($"Thrid foreach");
                        if (iteratortwo.Idemployee == iterator.Idemployee && iteratortwo.Type == 1)
                        {
                            log.LogInformation($"Is this the IDRowKey, {iterator.RowKey}, {iteratortwo.RowKey}");

                            WorkTimeEmpEntity work_time = new WorkTimeEmpEntity
                            {
                                Idemployee = iteratortwo.Idemployee,    
                                WorkingHour = iteratortwo.WorkingHour,
                                Type = iteratortwo.Type,
                                Consolidated = true,
                                PartitionKey = "WORK_TIME",
                                RowKey = iteratortwo.RowKey,
                                ETag = "*"
                            };

                            WorkTimeEmpEntity work_time_two = new WorkTimeEmpEntity
                            {
                                Idemployee = iterator.Idemployee,
                                WorkingHour = iterator.WorkingHour,
                                Type = iterator.Type,
                                Consolidated = true,
                                PartitionKey = "WORK_TIME",
                                RowKey = iterator.RowKey,
                                ETag = "*"
                            };

                            TableOperation updatework_time = TableOperation.Replace(work_time);
                            await timeTable.ExecuteAsync(updatework_time);

                            TableOperation updatework_time_two = TableOperation.Replace(work_time_two);
                            await timeTable.ExecuteAsync(updatework_time_two);
                            log.LogInformation($"Is this the calculated, {dateCalculated}");
                            await ValideWork(ConEntity, iterator, iteratortwo, dateCalculated, timeTable2);

                        }
                    }
                }
            }
        }

        public static async Task ValideWork(TableQuerySegment<ConsolEntity> consolEntity, WorkTimeEmpEntity work_time, WorkTimeEmpEntity work_time_two, TimeSpan dateCalculated, CloudTable timeTable2)
        {
            if (consolEntity.Results.Count == 0)
            {
                ConsolEntity ViewWorkTimeConsolited = new ConsolEntity
                {
                    IdEmployee = work_time.Idemployee,
                    DateTime = work_time.WorkingHour,
                    MinuteTime = dateCalculated.TotalMinutes,
                    PartitionKey = "WORK_CONDILATED",
                    RowKey = Guid.NewGuid().ToString(),
                    ETag = "*"
                };

                TableOperation insertCheckConsolidate = TableOperation.Insert(ViewWorkTimeConsolited);
                await timeTable2.ExecuteAsync(insertCheckConsolidate);
            }
            else
            {
                foreach (ConsolEntity Work_Cons in consolEntity)
                {
                    //log.LogInformation("Actualizando consolidado segunda tabla");
                    if (Work_Cons.IdEmployee == work_time.Idemployee)
                    {

                        ConsolEntity checkConsolidate = new ConsolEntity
                        {
                            IdEmployee = Work_Cons.IdEmployee,
                            DateTime = Work_Cons.DateTime,
                            MinuteTime = (double)(Work_Cons.MinuteTime + dateCalculated.TotalMinutes),
                            PartitionKey = Work_Cons.PartitionKey,
                            RowKey = Work_Cons.RowKey,
                            ETag = "*"
                        };

                        TableOperation insertConsolidate = TableOperation.Replace(checkConsolidate);
                        await timeTable2.ExecuteAsync(insertConsolidate);
                    }
                    else
                    {
                        ConsolEntity checkConsolidateFor = new ConsolEntity
                        {
                            IdEmployee = work_time.Idemployee,
                            DateTime = work_time.WorkingHour,
                            MinuteTime = dateCalculated.TotalMinutes,
                            PartitionKey = "WORKINGCONSOLIDATED",
                            RowKey = Guid.NewGuid().ToString(),
                            ETag = "*"
                        };

                        TableOperation insertConsolidate = TableOperation.Insert(checkConsolidateFor);
                        await timeTable2.ExecuteAsync(insertConsolidate);
                    }
                }
            }
        }
    }

}

