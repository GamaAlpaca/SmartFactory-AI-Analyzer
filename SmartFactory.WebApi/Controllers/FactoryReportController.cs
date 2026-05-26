using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SmartFactory.WebApi.Controllers
{
    // 這行是告訴系統，這個網頁的網址路徑是 http://localhost:5000/api/factoryreport
    [ApiController]
    [Route("api/[controller]")]
    public class FactoryReportController : ControllerBase
    {
        /// <summary>
        /// 網頁對外的窗口：當有人用瀏覽器連進來時，自動觸發
        /// </summary>
        [HttpGet("stagnant-materials")]
        public async Task<IActionResult> GetStagnantReport()
        {
            try
            {
                // 核心修正點：在撈資料之前，先叫隔壁棚把資料庫和表格蓋好、資料塞滿！
                // (把原本在主控台 Main 裡做的事，讓網頁一開啟也做一次，這樣就不怕沒表格了)
                SmartFactory.DataAnalyzer.Program.InitializeDatabase();

                // 1. 呼叫我們之前寫好的 Function 二：撈取呆滯無料數據
                // (注意：這裡直接呼叫隔壁專案的 Program 方法)
                string factoryData = SmartFactory.DataAnalyzer.Program.GetStagnantMaterials();

                // 2. 呼叫我們之前寫好的 Function 三：連線 Google Gemini 產出分析報告
                string aiReport = await SmartFactory.DataAnalyzer.Program.AnalyzeWithAI(factoryData);

                // 3. 用網頁標準的「200 OK」狀態，把這份香噴噴的報告吐回給瀏覽器
                return Ok(new
                {
                    Status = "Success",
                    Title = "智慧工廠呆滯料優化報告",
                    ReportContent = aiReport
                });
            }
            catch (System.Exception ex)
            {
                // 防呆：萬一連線外網或 DB 有問題，網頁不會死當，而是優雅吐出 500 錯誤訊息
                return StatusCode(500, new { Status = "Error", Message = ex.Message });
            }
        }
    }
}