#!/bin/bash

if [ ! "$#" = "2" ]; then
    echo "$0 <file> <id>" > /dev/fd/2
    exit 1
fi

file="$1"
id="$2"

if [ ! -f "$file" ]; then
    echo "file $file not found" > /dev/fd/2
    exit 1
fi

while IFS="," read -r date productId productCount customerCount salePrice; do
    if [ ! "$productId" = "\"$id\"" ]; then
        continue
    fi
    echo -n "{\"date\":$date,\"product_id\":${productId:1:-1},"
    echo -n "\"product_count\":${productCount:1:-1},\"customer_count\":${customerCount:1:-1},"
    echo "\"sale_price\":${salePrice:1:-1}}"
done < <(cat "$file" | tail -n+2)
