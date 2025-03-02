# DynamoDB Stream Lambda Function (.NET 8)

This repository contains a .NET 8 AWS Lambda function designed to process events from a DynamoDB Stream. The Lambda function logs information about each event to CloudWatch Logs, including the event ID, event name (INSERT, MODIFY, REMOVE), and the data (New and Old Images) associated with each event type.

## Code Overview

The Lambda function is written in C# using .NET 8 and utilizes the following AWS SDK for .NET packages:

*   **Amazon.Lambda.Core:**  Provides the core interfaces and base classes for building AWS Lambda functions.
*   **Amazon.Lambda.DynamoDBEvents:**  Provides classes for working with DynamoDB stream events in Lambda.
*   **AWSSDK.DynamoDBv2:**  Provides the AWS SDK for DynamoDB, although in this code, it's primarily used for `DynamoDBContext` initialization which isn't strictly necessary for stream processing itself but is included from your initial code.
*   **Amazon.Lambda.Serialization.SystemTextJson:**  Configures Lambda to use `System.Text.Json` for JSON serialization.

The `Function.cs` file contains the main Lambda function code:

*   **`Function` Class:**
    *   **`FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)`:** This is the entry point for the Lambda function. It processes each record in the `DynamoDBEvent`:
        *   Logs basic event information (ID, Name).
        *   Calls specific `ProcessInsert`, `ProcessModify`, or `ProcessRemove` methods based on the `EventName`.
    *   **`ProcessInsert(StreamRecord dynamodbRecord, ILambdaLogger logger)`:**  Logs the `NewImage` data for INSERT events.
    *   **`ProcessModify(StreamRecord dynamodbRecord, ILambdaLogger logger)`:** Logs both `NewImage` and `OldImage` data for MODIFY events.
    *   **`ProcessRemove(StreamRecord dynamodbRecord, ILambdaLogger logger)`:** Logs the `OldImage` data for REMOVE events.

## Prerequisites

Before deploying this Lambda function, ensure you have the following prerequisites:

*   **AWS Account:** You need an active AWS account.
*   **.NET 8 SDK:**  Install the .NET 8 SDK on your development machine. You can download it from [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0).
*   **AWS CLI:**  Install and configure the AWS Command Line Interface (CLI). See [https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2.html](https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2.html) for installation instructions.
*   **DynamoDB Table with Streams Enabled:** You need an existing DynamoDB table with DynamoDB Streams enabled.  When enabling streams, choose either "New image" or "New and old images" as the stream view type, depending on your requirements for capturing data.

## Setup and Deployment

Follow these steps to deploy the Lambda function:

1.  **Clone the Repository:** Clone this GitHub repository to your local machine.
2.  **Publish the Lambda Function:** Open a terminal in the project directory and run the following command to publish the Lambda function:

    ```bash
    dotnet publish -c Release
    ```

    This command will create a deployment package in the `bin/Release/net8.0/publish` directory.
3.  **Create Lambda Function in AWS Console:**
    *   Go to the AWS Management Console and navigate to the Lambda service.
    *   Click **Create function**.
    *   Choose **Author from scratch**.
    *   **Function name:**  Enter a name for your Lambda function (e.g., `DynamoDBStreamLogger`).
    *   **Runtime:** Select **.NET 8**.
    *   **Architecture:** Choose `x86_64` or `arm64` based on your needs.
    *   **Permissions:**
        *   **Execution role:** Choose an existing IAM role that has permissions to write logs to CloudWatch and access DynamoDB Streams. If you don't have one, create a new role with the `AWSLambdaBasicExecutionRole` policy and add the following inline policy to grant DynamoDB Streams access:

            ```json
            {
                "Version": "2012-10-17",
                "Statement": [
                    {
                        "Effect": "Allow",
                        "Action": [
                            "dynamodb:DescribeStream",
                            "dynamodb:GetRecords",
                            "dynamodb:GetShardIterator",
                            "dynamodb:ListStreams"
                        ],
                        "Resource": "arn:aws:dynamodb:<region>:<account-id>:table/<your-dynamodb-table-name>/stream/*"
                    }
                ]
            }
            ```

            *(Replace `<region>`, `<account-id>`, and `<your-dynamodb-table-name>` with your AWS region, account ID, and DynamoDB table name.)*
    *   Click **Create function**.
4.  **Configure Lambda Function Code:**
    *   In your Lambda function's configuration, go to the **Code** tab.
    *   **Upload from:** Select **.zip file**.
    *   **Upload:**  Click **Upload** and choose the `.zip` file located in the `bin/Release/net8.0/publish` directory of your project.
    *   **Handler:**  Enter `DynamodbStreamLambda::DynamodbStreamLambda.Function::FunctionHandler`
    *   Click **Save**.
5.  **Add DynamoDB Stream Trigger:**
    *   Go to the **Configuration** tab and select **Triggers**.
    *   Click **Add trigger**.
    *   **Trigger configuration:**
        *   **Trigger type:** Choose **DynamoDB**.
        *   **DynamoDB table:** Select your DynamoDB table that has streams enabled.
        *   **Batch size:**  (Optional) Adjust the batch size as needed.
        *   **Starting position:** Choose **TRIM_HORIZON** to process all existing records or **LATEST** for new records only.
        *   **Enabled:** Ensure the trigger is enabled.
    *   Click **Add**.

## Testing the Lambda Function

1.  **Make Changes to your DynamoDB Table:** Insert, modify, or delete items in your DynamoDB table that is connected to the stream.
2.  **Monitor CloudWatch Logs:**
    *   Go to the CloudWatch service in the AWS Management Console.
    *   Click **Log groups** in the left navigation pane.
    *   Find the log group for your Lambda function (usually named `/aws/lambda/<your-function-name>`).
    *   Click on the log group and then a log stream.
    *   You should see log messages indicating the processing of stream events, including details of the `INSERT`, `MODIFY`, and `REMOVE` events and the corresponding data.
