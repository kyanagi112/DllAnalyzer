import json
import psycopg2

# PostgreSQL に接続
conn = psycopg2.connect('host=localhost dbname=db_example user=postgres password=pass')
cur = conn.cursor()

# JSONファイルを読み込む
with open('method_calls.json', 'r', encoding='utf-8') as f:
    data = json.load(f)
    # データを挿入

    insert_count = 0

for entry in data:
    caller = entry.get("Caller")
    callees = entry.get("Callees", [])
    for callee in callees:
        cur.execute(
            "INSERT INTO method_calls (caller, callees) VALUES (%s, %s);",
            (caller, callee)
        )
        insert_count += 1

conn.commit()
print(f"{insert_count} rows inserted.")


#    for item in data:
#        caller = item['Caller']
#        callees = item['Callees']
#        cur.execute(
#            "INSERT INTO method_calls (caller, callees) VALUES (%s, %s)",
#            (caller, callees)
#        )

conn.commit()
cur.close()
conn.close()