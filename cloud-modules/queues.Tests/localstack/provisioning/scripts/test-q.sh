#!/bin/sh

TEST_Q="test-queue"
awslocal sqs create-queue --queue-name $TEST_Q

echo "A SQS queue ${TEST_Q} has been provisioned"