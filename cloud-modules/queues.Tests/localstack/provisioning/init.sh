#!/bin/sh -x

/scripts/test-sm.sh
/scripts/test-b.sh
/scripts/test-q.sh
/scripts/test-transconsumer.sh

exit 0
