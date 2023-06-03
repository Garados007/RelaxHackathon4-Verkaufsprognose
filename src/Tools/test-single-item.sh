#!/bin/bash

if [ ! "$#" = "1" ]; then
    echo "$0 <item id>" > /dev/fd/2
    exit 1
fi

id="$1"

top_level=$(git rev-parse --show-toplevel)

lines=()

while IFS= read -r line; do

    lines+=("$line")

done < <("$top_level/src/Tools/sales-filter.sh" "$top_level/vendor/data/sales_task.csv" "$id")

for line in "${lines[@]}"; do

    date="$(echo "$line" | jq -r ".date")"
    echo "################## $date ####################"
    echo ""

    # upload line to server
    curl -Ss -d "[$line]" -H 'Content-Type: application/json' "http://localhost:3000/sales"
    echo ""

    # get inventory
    curl -Ss "http://localhost:3000/inventory" | jq ".[\"$id\"]"
    echo ""

    # get stats
    curl -Ss "http://localhost:3000/stats?date=$date" | jq ".[\"$id\"]"
    echo ""

    # get orders
    curl -Ss "http://localhost:3000/orders?date=$date"
    echo ""

    # read -n 1 -s

done
