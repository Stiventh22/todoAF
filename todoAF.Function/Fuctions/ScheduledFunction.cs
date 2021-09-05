using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;
using todoAF.Function.Entities;

namespace todoAF.Function.Fuctions
{
    public static class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async Task Run(
            [TimerTrigger("0 */1 * * * *")] TimerInfo myTimer,
            [Table("todo", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log)
        {
            // CheckEntity = Tabla 1
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<TodoEntity> query = new TableQuery<TodoEntity>().Where(filter);
            TableQuerySegment<TodoEntity> allCheckEntity = await todoTable.ExecuteQuerySegmentedAsync(query, null);

            //CheckConsolidateEntity = Tabla 2
            //TableQuery<ConsolidateEntity> queryConsolidate = new TableQuery<ConsolidateEntity>();
            //TableQuerySegment<ConsolidateEntity> allCheckConsolidateEntity = await workingTimeTable.ExecuteQuerySegmentedAsync(queryConsolidate, null);

            bool correctUpdate = false;

            log.LogInformation($"Entrando al primer foreach");
            foreach (TodoEntity item in allCheckEntity)
            {
                log.LogInformation($"Este es el primer if");
                if (!string.IsNullOrEmpty(item.Idemployee.ToString()) && item.Type == 0)
                {
                    log.LogInformation($"Este es el segundo foreach");
                    foreach (TodoEntity itemtwo in allCheckEntity)
                    {
                        TimeSpan dateCalculated = (itemtwo.WorkingHour - item.WorkingHour);
                        log.LogInformation($"Este es el tercer foreach");
                        if (itemtwo.Idemployee.Equals(item.Idemployee) && itemtwo.Type == 1)
                        {
                            log.LogInformation($"Este es el IDRowKey, {item.RowKey}, {itemtwo.RowKey}");

                            TodoEntity check = new TodoEntity
                            {
                                Idemployee = itemtwo.Idemployee,
                                WorkingHour = Convert.ToDateTime(dateCalculated.ToString()),
                                Type = itemtwo.Type,
                                Consolidated = true,
                                PartitionKey = "WORKINGTIME",
                                RowKey = itemtwo.RowKey,
                                ETag = "*"
                            };

                            log.LogInformation($"Este es el cálculo, {dateCalculated}");
                            TableOperation updateCheckEntity = TableOperation.Replace(check);
                            await todoTable.ExecuteAsync(updateCheckEntity);
                            correctUpdate = true;
                        }

                        log.LogInformation($"He estado aquí, {item.RowKey}");
                        if (correctUpdate == true)
                        {
                            TodoEntity check = new TodoEntity
                            {
                                Idemployee = item.Idemployee,
                                WorkingHour = Convert.ToDateTime(dateCalculated.ToString()),
                                Type = item.Type,
                                Consolidated = true,
                                PartitionKey = "WORKINGTIME",
                                RowKey = item.RowKey,
                                ETag = "*"
                            };
                            TableOperation updateCheckEntity = TableOperation.Replace(check);
                            await todoTable.ExecuteAsync(updateCheckEntity);
                        }
                    }
                }
            }
        }


    }
}
