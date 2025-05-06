import psycopg2
import csv

# PostgreSQLの接続情報
DB_HOST = 'localhost'
DB_PORT = '5432'
DB_NAME = 'db_example'
DB_USER = 'postgres'
DB_PASSWORD = 'pass'

# 実行するSQL文
SQL_QUERY = 'WITH RECURSIVE expcall(caller, callees, level, path) AS ('  \
            'SELECT ' \
            'caller,callees,1 AS level,caller || \'→\' || callees AS path ' \
            'FROM method_calls ' \
            'UNION ALL ' \
            'SELECT expcall.caller,method_calls.callees,expcall.level + 1,expcall.path || \'→\' || method_calls.callees ' \
            'FROM expcall ' \
            'JOIN method_calls ' \
            'ON expcall.callees = method_calls.caller ) ' \
            'SELECT * FROM expcall;'


# 出力するCSVファイル名
CSV_FILE = 'output.csv'

def export_to_csv():
    try:
        # PostgreSQLに接続
        connection = psycopg2.connect(
            host=DB_HOST,
            port=DB_PORT,
            dbname=DB_NAME,
            user=DB_USER,
            password=DB_PASSWORD
        )
        cursor = connection.cursor()

        # SQLを実行
        cursor.execute(SQL_QUERY)
        rows = cursor.fetchall()

        # カラム名を取得
        col_names = [desc[0] for desc in cursor.description]

        # CSVに出力
        with open(CSV_FILE, mode='w', newline='', encoding='utf-8-sig') as file:
            writer = csv.writer(file)
            writer.writerow(col_names)  # ヘッダー
            writer.writerows(rows)      # データ

        print(f"✅ データをCSVファイル '{CSV_FILE}' に出力しました。")

    except Exception as e:
        print(f"⚠️ エラーが発生しました: {e}")

    finally:
        if cursor:
            cursor.close()
        if connection:
            connection.close()

if __name__ == "__main__":
    export_to_csv()
