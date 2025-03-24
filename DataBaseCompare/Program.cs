using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DataBaseCompare.Models;
using DataBaseCompare.Services;

namespace DataBaseCompare
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var settings = configuration.GetSection("MigrationSettings").Get<MigrationSettings>();
                var compareSettings = configuration.GetSection("CompareSettings").Get<CompareSettings>();
                var sourceConnectionString = configuration.GetConnectionString("SourceDatabase");
                var targetConnectionString = configuration.GetConnectionString("TargetDatabase");

                var migrationService = new DatabaseMigrationService(
                    sourceConnectionString,
                    targetConnectionString,
                    settings,
                    compareSettings
                );

                Console.WriteLine("请选择操作：");
                Console.WriteLine("1. 数据迁移");
                Console.WriteLine("2. 数据对比");
                Console.Write("请输入选项（1或2）：");

                var choice = Console.ReadLine();

                if (choice == "1")
                {
                    Console.WriteLine("开始数据迁移...");
                    await migrationService.MigrateDataAsync();
                    Console.WriteLine("数据迁移完成！");
                }
                else if (choice == "2")
                {
                    Console.WriteLine("开始数据对比...");
                    await migrationService.CompareDataAsync();
                    Console.WriteLine("数据对比完成！");
                }
                else
                {
                    Console.WriteLine("无效的选项！");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
        }
    }
}
