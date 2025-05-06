WITH RECURSIVE expcall(caller, callees, level, path) AS (
    -- 初期（呼び出し元からから直接の呼び出し先）
    SELECT
        caller,
        callees,
        1 AS level,
        caller || '→' || callees AS path
    FROM method_calls

    UNION ALL

    -- 再帰（呼び出し先をさらに展開）
    SELECT
        expcall.caller,
        method_calls.callees,
        expcall.level + 1,
        expcall.path || '→' || method_calls.callees
    FROM expcall
    JOIN method_calls
        ON expcall.callees = method_calls.caller
    )

SELECT * FROM expcall
ORDER BY caller, callees;