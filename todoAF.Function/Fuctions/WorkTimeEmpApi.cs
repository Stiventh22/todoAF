using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using WorkTimeEmp.common.Responses;
using WorkTimeEmp.Function.Entities;

namespace WorkTimeEmp.Function.Fuctions
{
    public static class WorkTimeEmpApi
    {

        [FunctionName(nameof(CreateWorkTimeEmp))]
        public static async Task<IActionResult> CreateWorkTimeEmp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "WorkTimeEmp")] HttpRequest req,
            [Table("WorkTimeEmpEntity", Connection = "AzureWebJobsStorage")] CloudTable WorkTable,
            ILogger log)
        {
            log.LogInformation("A new employee has entered");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            WorkTimeEmpEntity workTimeEmp = JsonConvert.DeserializeObject<WorkTimeEmpEntity>(requestBody);
            if (string.IsNullOrEmpty(workTimeEmp?.Idemployee.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "The request must have a employee ID."
                });
            }

            WorkTimeEmpEntity workTimeEmpEntity = new WorkTimeEmpEntity
            {
                Idemployee = workTimeEmp.Idemployee,
                WorkingHour = DateTime.UtcNow,
                Type = workTimeEmp.Type,
                Consolidated = false,
                PartitionKey = "WORK",
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*"

            };

            TableOperation addOperation = TableOperation.Insert(workTimeEmpEntity);
            await WorkTable.ExecuteAsync(addOperation);

            string message = "New employee stored in table";
            log.LogInformation(message);



            return new OkObjectResult(new Response
            {
                Idemployee = workTimeEmp.Idemployee,
                WorkingHour = DateTime.UtcNow,
                Message = message,
                Result = workTimeEmpEntity
            });
        }

        [FunctionName(nameof(UpdateWorkTimeEmp))]
        public static async Task<IActionResult> UpdateWorkTimeEmp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "WorkTimeEmp/{IdEmployee}")] HttpRequest req,
            [Table("WorkTimeEmpEntity", Connection = "AzureWebJobsStorage")] CloudTable WorkTable,
            string IdEmployee,
            ILogger log)


        {
            log.LogInformation($"Update for ID: {IdEmployee}, received.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            WorkTimeEmpEntity workTimeEmpEntity = JsonConvert.DeserializeObject<WorkTimeEmpEntity>(requestBody);

            //Validate Working Hour ID for employee      

            TableOperation findOperation = TableOperation.Retrieve<WorkTimeEmpEntity>("WORK", IdEmployee);
            TableResult findResult = await WorkTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "Employee ID: {IdEmployee}  not found."
                });
            }

            // Update Working Hour

            WorkTimeEmpEntity workTimeEmpEntity1 = (WorkTimeEmpEntity)findResult.Result;
            workTimeEmpEntity.WorkingHour = workTimeEmpEntity.WorkingHour;
            workTimeEmpEntity.Type = workTimeEmpEntity.Type;

            if (!string.IsNullOrEmpty(workTimeEmpEntity?.Type.ToString()))
            {
                workTimeEmpEntity.Type = workTimeEmpEntity.Type;

            }

            TableOperation addOperation = TableOperation.Replace(workTimeEmpEntity);
            await WorkTable.ExecuteAsync(addOperation);

            string message = $"Update a register in table, id: {IdEmployee}";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                Idemployee = workTimeEmpEntity.Idemployee,
                WorkingHour = DateTime.UtcNow,
                Message = message,
                Result = WorkTable

            });
        }
        [FunctionName(nameof(GetAllWorkTimeEmp))]
        public static async Task<IActionResult> GetAllWorkTimeEmp(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "WorkTimeEmp")] HttpRequest req,
           [Table("WorkTimeEmpEntity", Connection = "AzureWebJobsStorage")] CloudTable WorkTable,
           ILogger log)
        {
            log.LogInformation("Get all Works received.");

            TableQuery<WorkTimeEmpEntity> query = new TableQuery<WorkTimeEmpEntity>();
            TableQuerySegment<WorkTimeEmpEntity> works = await WorkTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all Works.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                Message = message,
                Result = works
            });
        }

        [FunctionName(nameof(GetWorkTimeEmpById))]
        public static IActionResult GetWorkTimeEmpById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "WorkTimeEmp/{id}")] HttpRequest req,
            [Table("WorkTimeEmpEntity", "WORK", "{id}", Connection = "AzureWebJobsStorage")] WorkTimeEmpEntity workTimeEmpEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get Work by id: {id}, received.");

            if (workTimeEmpEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "Work not found."
                });
            }

            string message = $"Work: {workTimeEmpEntity.RowKey}, retrieved.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                Message = message,
                Result = workTimeEmpEntity
            });
        }

        [FunctionName(nameof(DeleteWorkTimeEmp))]
        public static async Task<IActionResult> DeleteWorkTimeEmp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "WorkTimeEmp/{id}")] HttpRequest req,
            [Table("WorkTimeEmpEntity", "WORK", "{id}", Connection = "AzureWebJobsStorage")] WorkTimeEmpEntity workTimeEmpEntity,
            [Table("WorkTimeEmpEntity", Connection = "AzureWebJobsStorage")] CloudTable WorkTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Delete Work: {id}, received.");

            if (workTimeEmpEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "Work not found."
                });
            }

            await WorkTable.ExecuteAsync(TableOperation.Delete(workTimeEmpEntity));
            string message = $"Work: {workTimeEmpEntity.RowKey}, deleted.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                Message = message,
                Result = workTimeEmpEntity
            });
        }
    }
}


