using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;

// メソッド呼び出し情報を格納するクラス
class MethodCallInfo
{ 
    // Caller: 呼び出し元メソッドの情報
    public string Caller { get; set; } = string.Empty;
    // Callees: 呼び出し先メソッドの情報のリスト
    public List<string> Callees { get; set; } = new List<string>();
}
// DllAnalyzer クラス: DLL を解析してメソッド呼び出しを抽出する
class DllAnalyzer
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("使い方: DllAnalyzer.exe <path-to-dll>");
            return;
        }

        string dllPath = args[0];
        if (!File.Exists(dllPath))
        {
            Console.WriteLine("DLL が見つかりません: " + dllPath);
            return;
        }

        // Mono.Cecil を使用して DLL を読み込む
        var assembly = AssemblyDefinition.ReadAssembly(dllPath);
        // メソッド呼び出し情報を格納するリスト
        // MethodCallInfo クラスのインスタンスを格納するリスト  
        var methodCalls = new List<MethodCallInfo>();

        // 各モジュールをループして、型を取得する
        foreach (var module in assembly.Modules)
        {
            // 各モジュール内の型をループして、メソッド呼び出しを解析する
            foreach (var type in module.Types)
            {
                // ProcessType メソッドを呼び出して、型内のメソッド呼び出しを解析する
                ProcessType(type, methodCalls);
            }
        }  
        string jsonOutput = JsonSerializer.Serialize(methodCalls, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("method_calls.json", jsonOutput);

        Console.WriteLine("解析完了！JSON 出力: method_calls.json");
    }

    // 型内のメソッド呼び出しを解析するメソッド
    static void ProcessType(TypeDefinition type, List<MethodCallInfo> methodCalls)
    {
        // 型が抽象クラスまたはインターフェースの場合はスキップ
        foreach (var method in type.Methods)
        {
            // メソッドが抽象メソッドまたはインターフェースメソッドの場合はスキップ
            if (method.IsAbstract || method.IsVirtual && method.IsHideBySig) continue;
            // メソッドがボディを持たない場合はスキップ
            if (!method.HasBody) continue;
            // メソッドが IL コードを持たない場合はスキップ
            if (method.Body.Instructions.Count == 0) continue;  
            var info = new MethodCallInfo            
            {
                // 呼び出し元メソッドの情報を格納するプロパティ
                Caller = $"{type.FullName}.{method.Name}({string.Join(", ", method.Parameters.Select(p => p.ParameterType.Name))})"
            };

            // メソッドの IL コードをループして、呼び出し先メソッドを抽出する
            foreach (var instr in method.Body.Instructions)
            {

                // Call または Callvirt 命令をチェック
                // OpCode が Call または Callvirt の場合、呼び出し先メソッドを取得する  
                if (instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt)
                {
                    // Operand が MethodReference 型の場合、呼び出し先メソッドの情報を取得する
                    // MethodReference 型の Operand を取得し、呼び出し先メソッドの情報を格納する    
                    if (instr.Operand is MethodReference calledMethod)
                    {
                        // 呼び出し先メソッドの情報を格納するプロパティ
                        // 呼び出し先メソッドの情報を格納するプロパティに、呼び出し元メソッドの情報を格納するプロパティを追加する   
                        info.Callees.Add($"{calledMethod.DeclaringType.FullName}.{calledMethod.Name}({string.Join(", ", calledMethod.Parameters.Select(p => p.ParameterType.Name))})");
                    }
                }
            }
            // 呼び出し先メソッドが存在する場合、メソッド呼び出し情報をリストに追加する
            if (info.Callees.Count > 0)
            {   
                methodCalls.Add(info);
            }
        }
        // ネストされた型にも対応
        foreach (var nested in type.NestedTypes)
        {
            ProcessType(nested, methodCalls);
        }
    }
}
