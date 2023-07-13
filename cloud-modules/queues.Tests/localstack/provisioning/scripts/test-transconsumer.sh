#!/bin/sh

TEST_TCB="s3://test-transconsumer-bucket"
awslocal s3 mb $TEST_TCB
echo "A S3 bucket ${TEST_TCB} has been provisioned"

TEST_TCQ="test-transconsumer-queue"
awslocal sqs create-queue --queue-name $TEST_TCQ
echo "A SQS queue ${TEST_TCQ} has been provisioned"
