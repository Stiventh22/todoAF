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
            [Table("WorkTimeEmp", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            [Table("WORKING", Connection = "AzureWebJobsStorage")] CloudTable timeTable2,
            ILogger log)
        {
            //Table 1
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<WorkTimeEmpEntity> query = new TableQuery<WorkTimeEmpEntity>().Where(filter);
            TableQuerySegment<WorkTimeEmpEntity> allCheckEntity = await timeTable.ExecuteQuerySegmentedAsync(query, null);

            //Table 2
            //TableQuery<ConsolidateEntity> queryConsolidate = new TableQuery<ConsolidateEntity>();
            //TableQuerySegment<ConsolidateEntity> allCheckConsolidateEntity = await workingTimeTable.ExecuteQuerySegmentedAsync(queryConsolidate, null);

            bool correctUpdate = false;

            log.LogInformation($"First cicle");
            foreach (WorkTimeEmpEntity item in allCheckEntity)
            {
                log.LogInformation($"First if");
                if (!string.IsNullOrEmpty(item.Idemployee.ToString()) && item.Type == 0)
                {
                    log.LogInformation($"Second foreach");
                    foreach (WorkTimeEmpEntity itemtwo in allCheckEntity)
                    {
                        TimeSpan dateCalculated = (itemtwo.WorkingHour - item.WorkingHour);
                        log.LogInformation($"Thrid foreach");
                        if (itemtwo.Idemployee.Equals(item.Idemployee) && itemtwo.Type == 1)
                        {
                            log.LogInformation($"Is this the IDRowKey, {item.RowKey}, {itemtwo.RowKey}");

                            WorkTimeEmpEntity check = new WorkTimeEmpEntity
                            {
                                Idemployee = itemtwo.Idemployee,    
                                WorkingHour = Convert.ToDateTime(dateCalculated.ToString()),
                                Type = itemtwo.Type,
                                Consolidated = true,
                                PartitionKey = "WORK",
                                RowKey = itemtwo.RowKey,
                                ETag = "*"
                            };

                            log.LogInformation($"Is this the calculated, {dateCalculated}");
                            TableOperation updateCheckEntity = TableOperation.Replace(check);
                            await timeTable.ExecuteAsync(updateCheckEntity);
                            correctUpdate = true;
                        }

                        log.LogInformation($"I've been here, {item.RowKey}");
                        if (correctUpdate == true)
                        {
                            WorkTimeEmpEntity check = new WorkTimeEmpEntity
                            {
                                Idemployee = item.Idemployee,
                                WorkingHour = Convert.ToDateTime(dateCalculated.ToString()),
                                Type = item.Type,
                                Consolidated = true,
                                PartitionKey = "WORK",
                                RowKey = item.RowKey,
                                ETag = "*"
                            };
                            TableOperation updateCheckEntity = TableOperation.Replace(check);
                            await timeTable.ExecuteAsync(updateCheckEntity);
                        }
                    }
                }
            }
        }


    }
}
