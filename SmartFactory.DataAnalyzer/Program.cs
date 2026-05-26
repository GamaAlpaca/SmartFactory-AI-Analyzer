using Microsoft.Data.Sqlite;
using Microsoft.Extensions.AI; // 引入微軟官方 AI 套件
using Mscc.GenerativeAI;       // 引入 Gemini 專用套件
using Mscc.GenerativeAI.Microsoft;
using System;
using System.Threading.Tasks; // 處理 AI 連線需要用到非同步 Async

namespace SmartFactory.DataAnalyzer
{
    public class Program
    {
        private static readonly string ConnectionString = "Data Source=FactoryData.db";

        // ⚠️ 請把妳剛剛複製的 AIzaSy 開頭鑰匙，貼在下面這個雙引號裡面
        // 🔒 修正：絕對不寫死，改成從操作系統的環境變數（Environment Variable）去抓取
        private static readonly string ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            ?? throw new InvalidOperationException("錯誤：找不到環境變數 GEMINI_API_KEY，請先設定它！");

        // 因為呼叫 AI 是網路連線，Main 方法要改成 static async Task
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== 智慧工廠資料分析器：啟動 ===");

            // 1. 初始化資料庫（Function 一）
            InitializeDatabase();

            // 2. 撈取並組裝呆滯料資料（Function 二）
            string dataForAI = GetStagnantMaterials();

            // 3. 【全新步驟】呼叫 AI 進行智慧分析（Function 三）
            Console.WriteLine("\n[連線日誌] 正在將數據發送給 Google Gemini 大腦，請稍候...");
            string aiReport = await AnalyzeWithAI(dataForAI);

            // 4. 印出 AI 幫妳寫好的報告
            Console.WriteLine("\n====================================");
            Console.WriteLine("[Gemini AI 產出的工廠優化報告]：");
            Console.WriteLine(aiReport);
            Console.WriteLine("====================================");

            Console.WriteLine("\n=== 專案核心流程大成功！請按任意鍵結束 ===");
            //Console.ReadKey();
        }

        public static void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS MaterialInventory (
                        Item_No TEXT PRIMARY KEY,
                        Qty INTEGER,
                        Update_Date TEXT
                    );";
                using (var command = new SqliteCommand(createTableSql, connection)) { command.ExecuteNonQuery(); }

                string insertDataSql = @"
                    INSERT OR IGNORE INTO MaterialInventory (Item_No, Qty, Update_Date) VALUES 
                    ('A001', 500, '2025-01-10'),
                    ('B002', 120, '2026-05-20'),
                    ('C003', 800, '2025-03-15');";
                using (var command = new SqliteCommand(insertDataSql, connection)) { command.ExecuteNonQuery(); }
            }
        }

        public static string GetStagnantMaterials()
        {
            string resultText = "工廠內部重要數據摘要：\n";
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                string querySql = @"
                    SELECT Item_No, Qty, Update_Date 
                    FROM MaterialInventory 
                    WHERE Qty > 0 AND Update_Date <= '2025-12-31'";

                using (var command = new SqliteCommand(querySql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string itemNo = reader["Item_No"].ToString();
                        int qty = Convert.ToInt32(reader["Qty"]);
                        string updateDate = reader["Update_Date"].ToString();
                        resultText += $"- 品號: {itemNo}, 目前庫存: {qty} 個, 最後異動時間: {updateDate}。\n";
                    }
                }
            }
            return resultText;
        }

        /// <summary>
        /// Function 三：最新查證版—使用微軟官方標準 GeminiChatClient
        /// </summary>
        public static async Task<string> AnalyzeWithAI(string factoryData)
        {
            // 1. 修正 CS0121：加上具名引數 (apiKey, model)，明確告訴編譯器參數的對應關係
            IChatClient chatClient = new GeminiChatClient(apiKey: ApiKey, model: "gemini-2.5-flash");

            // 2. 設計妳的 Prompt
            string prompt = $@"
    妳是一位專業的生管（PC）與資深倉管主管。
    以下是工廠目前撈出的潛在呆滯無料數據：
    {factoryData}

    請針對這些數據，用『繁體中文』提供一份簡短的分析報告，內容必須包含：
    1. 簡單說明為什麼這兩筆資料被判定為呆滯。
    2. 給予生管或採購部門具體的後續處置建議（例如：轉作其他機種、銷退、折讓或報廢）。
    請用條列式呈現，語氣要專業、果斷。";

            // 3. 修正 CS1061：改傳原生的 ChatMessage 陣列，避開字串擴充方法找不到的問題
            var chatMessages = new[]
            {
                new ChatMessage(ChatRole.User, prompt)
            };

            // 🌟 核心修正點：將舊版的 CompleteAsync 改為微軟最新標準化方法 GetResponseAsync
            var response = await chatClient.GetResponseAsync(chatMessages);

            // 4. 精準回傳 AI 寫好的報告文字
            // (註：如果 response.Message.Text 還是報錯，可以將這行改成 return response.Text; 即可)
            return response.Text;
        }
    }
}