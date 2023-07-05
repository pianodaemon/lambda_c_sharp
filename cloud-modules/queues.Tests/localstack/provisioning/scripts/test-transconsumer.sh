#!/bin/sh

TEST_TCQ="test-transconsumer-queue"
awslocal sqs create-queue --queue-name $TEST_TCQ

echo "A SQS queue ${TEST_TCQ} has been provisioned"
