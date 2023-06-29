#!/bin/sh

TEST_B="s3://test-bucket"
awslocal s3 mb $TEST_B

echo "A S3 bucket ${TEST_B} has been provisioned"