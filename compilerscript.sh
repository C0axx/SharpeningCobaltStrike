#!/bin/bash
/usr/bin/dotnet $1 --source-file $2 --file $3 --confuse --dotnet-framework $4 --output-kind $5 2>&1 > /dev/null
