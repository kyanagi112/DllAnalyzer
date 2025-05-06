import json
import psycopg

# PostgreSQL に接続
conn_string = "host=localhost dbname=db_example user=postgres password=pass"
conn = psycopg.connect(conn_string)

cur = conn.cursor()

# JSONファイルを読み込む
with open('bin/Debug/net9.0/method_calls.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

# データを挿入
for item in data:
    caller = item['Caller']
    callees = item['Callees']
    cur.execute(
        "INSERT INTO method_calls (caller, callees) VALUES (%s, %s)",
        (caller, callees)
    )

conn.commit()
cur.close()
conn.close()