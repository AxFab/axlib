#!/bin/bash


LICENSE=$(cat LICENSE_START.txt |  tr '\r' '¶')
LN=$(cat LICENSE_START.txt | wc -l)

update() {

    FILE="$1"
    START=$(head -n "$LN" "$FILE" | tr '\r' '¶')

    if [ "$LICENSE" != "$START" ]; then
        TMP=$(mktemp)
        cat "LICENSE_START.txt" "${FILE}" > "${TMP}"
        mv "${TMP}" "${FILE}"
        echo "${FILE}: Fixed"
    else
        echo "${FILE}: Ok"
    fi
}


for src in $( find . -name '*.cs' | grep -v '/obj/'); do
    update "$src"
done
