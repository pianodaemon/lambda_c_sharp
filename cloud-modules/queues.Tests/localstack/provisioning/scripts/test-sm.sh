#!/bin/sh

TEST_SECRET="sheldon-cooper-says"
TEST_SECRET_VALUE="bazinga"
awslocal secretsmanager create-secret --name $TEST_SECRET --secret-string $TEST_SECRET_VALUE

echo "A Secret known as ${TEST_SECRET} has been provisioned"
