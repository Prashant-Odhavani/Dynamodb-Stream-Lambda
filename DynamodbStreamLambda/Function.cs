using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using System.Text.Json;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DynamodbStreamLambda;

public class Function
{
    private readonly DynamoDBContext _context;

    public Function()
    {
        _context = new DynamoDBContext(new AmazonDynamoDBClient());
    }

    public void FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        context.Logger.LogInformation($"Beginning to process {dynamoEvent.Records.Count} records...");

        foreach (var record in dynamoEvent.Records)
        {
            context.Logger.LogInformation($"Event ID: {record.EventID}");
            context.Logger.LogInformation($"Event Name: {record.EventName}");

            if (record.EventName == "INSERT")
            {
                context.Logger.LogInformation("INSERT event detected");
                ProcessInsert(record.Dynamodb, context.Logger);
            }
            else if (record.EventName == "MODIFY")
            {
                context.Logger.LogInformation("MODIFY event detected");
                ProcessModify(record.Dynamodb, context.Logger);
            }
            else if (record.EventName == "REMOVE")
            {
                context.Logger.LogInformation("REMOVE event detected");
                ProcessRemove(record.Dynamodb, context.Logger);
            }
            else
            {
                context.Logger.LogInformation($"Event name: {record.EventName} not processed");
            }
        }

        context.Logger.LogInformation("Stream processing complete.");
    }

    private void ProcessInsert(StreamRecord dynamodbRecord, ILambdaLogger logger)
    {
        if (dynamodbRecord.NewImage != null && dynamodbRecord.NewImage.Count > 0)
        {
            logger.LogInformation("New Image Data:");
            foreach (var attribute in dynamodbRecord.NewImage)
            {
                logger.LogInformation($"Attribute Name: {attribute.Key}, Value: {JsonSerializer.Serialize(attribute.Value)}");
            }
        }
    }

    private void ProcessModify(StreamRecord dynamodbRecord, ILambdaLogger logger)
    {
        if (dynamodbRecord.NewImage != null && dynamodbRecord.NewImage.Count > 0)
        {
            logger.LogInformation("New Image Data (MODIFY):");
            foreach (var attribute in dynamodbRecord.NewImage)
            {
                logger.LogInformation($"Attribute Name: {attribute.Key}, Value: {JsonSerializer.Serialize(attribute.Value)}");
            }
        }

        if (dynamodbRecord.OldImage != null && dynamodbRecord.OldImage.Count > 0)
        {
            logger.LogInformation("Old Image Data (MODIFY):");
            foreach (var attribute in dynamodbRecord.OldImage)
            {
                logger.LogInformation($"Attribute Name: {attribute.Key}, Value: {JsonSerializer.Serialize(attribute.Value)}");
            }
        }
    }

    private void ProcessRemove(StreamRecord dynamodbRecord, ILambdaLogger logger)
    {
        if (dynamodbRecord.OldImage != null && dynamodbRecord.OldImage.Count > 0)
        {
            logger.LogInformation("Old Image Data (REMOVE):");
            foreach (var attribute in dynamodbRecord.OldImage)
            {
                logger.LogInformation($"Attribute Name: {attribute.Key}, Value: {JsonSerializer.Serialize(attribute.Value)}");
            }
        }
    }
}