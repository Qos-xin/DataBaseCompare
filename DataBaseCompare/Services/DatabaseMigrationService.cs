using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using DataBaseCompare.Models;

namespace DataBaseCompare.Services
{
    public class DatabaseMigrationService
    {
        private readonly string _sourceConnectionString;
        private readonly string _targetConnectionString;
        private readonly MigrationSettings _settings;
        private readonly CompareSettings _compareSettings;

        public DatabaseMigrationService(string sourceConnectionString, string targetConnectionString, MigrationSettings settings, CompareSettings compareSettings)
        {
            _sourceConnectionString = sourceConnectionString;
            _targetConnectionString = targetConnectionString;
            _settings = settings;
            _compareSettings = compareSettings;
        }

        public async Task MigrateDataAsync()
        {
            foreach (var table in _settings.Tables)
            {
                Console.WriteLine($"开始迁移表 {table.Name}...，按回车继续");
                Console.ReadKey();
                await MigrateTableAsync(table);
                Console.WriteLine($"表 {table.Name} 迁移完成");
            }
        }

        public async Task CompareDataAsync()
        {
            Console.WriteLine("开始数据对比...");
            using var sourceConnection = new MySqlConnection(_sourceConnectionString);
            using var targetConnection = new MySqlConnection(_targetConnectionString);

            // 获取指定时间范围内的订单号
            var orderQuery = $@"
                SELECT DISTINCT `{_compareSettings.LogTable.OrderNoColumn}` 
                FROM `{_compareSettings.LogTable.Name}`
                WHERE `{_compareSettings.LogTable.DateColumn}` BETWEEN @StartDate AND @EndDate";

            var parameters = new { 
                StartDate = _compareSettings.CompareDateRange.StartDate, 
                EndDate = _compareSettings.CompareDateRange.EndDate 
            };

            var orderNos = await sourceConnection.QueryAsync<string>(orderQuery, parameters);
            Console.WriteLine($"找到 {orderNos.Count()} 个订单号");

            foreach (var orderNo in orderNos)
            {
                await CompareOrderAsync(sourceConnection, targetConnection, orderNo);
            }
        }

        private async Task CompareOrderAsync(MySqlConnection sourceConnection, MySqlConnection targetConnection, string orderNo)
        {
            foreach (var table in _compareSettings.Tables)
            {
                var sourceQuery = $@"
                    SELECT * FROM `{table.Name}`
                    WHERE `{table.KeyColumn}` = @OrderNo";

                var sourceData = await sourceConnection.QueryAsync(sourceQuery, new { OrderNo = orderNo });
                var targetData = await targetConnection.QueryAsync(sourceQuery, new { OrderNo = orderNo });

                if (sourceData.Count() != targetData.Count())
                {
                    Console.WriteLine($"订单 {orderNo} 在表 {table.Name} 中的数据不一致：");
                    Console.WriteLine($"源数据库记录数：{sourceData.Count()}");
                    Console.WriteLine($"目标数据库记录数：{targetData.Count()}");
                }
                else if (sourceData.Count() > 0)
                {
                    var sourceDict = ((IDictionary<string, object>)sourceData.First());
                    var targetDict = ((IDictionary<string, object>)targetData.First());

                    var differences = new List<string>();
                    foreach (var key in sourceDict.Keys)
                    {
                        if (!targetDict.ContainsKey(key) || !Equals(sourceDict[key], targetDict[key]))
                        {
                            differences.Add($"字段 {key}: 源={sourceDict[key]}, 目标={(targetDict.ContainsKey(key) ? targetDict[key] : "不存在")}");
                        }
                    }

                    if (differences.Any())
                    {
                        Console.WriteLine($"订单 {orderNo} 在表 {table.Name} 中的数据不一致：");
                        foreach (var diff in differences)
                        {
                            Console.WriteLine(diff);
                        }
                    }
                }
            }
        }

        private async Task MigrateTableAsync(TableConfig table)
        {
            using var sourceConnection = new MySqlConnection(_sourceConnectionString);
            using var targetConnection = new MySqlConnection(_targetConnectionString);

            var query = $@"
                SELECT * FROM `{table.Name}`
                WHERE `{table.DateColumn}` BETWEEN @StartDate AND @EndDate";

            var parameters = new { StartDate = _settings.StartDate, EndDate = _settings.EndDate };
            var sourceData = await sourceConnection.QueryAsync(query, parameters);

            foreach (var row in sourceData)
            {
                var keyValue = ((IDictionary<string, object>)row)[table.KeyColumn];
                var exists = await CheckRecordExistsAsync(targetConnection, table, keyValue);

                if (!exists)
                {
                    await InsertRecordAsync(targetConnection, table, (IDictionary<string, object>)row);
                    Console.WriteLine($"插入记录: {table.Name} - {keyValue}");
                }
            }
        }

        private async Task<bool> CheckRecordExistsAsync(MySqlConnection connection, TableConfig table, object keyValue)
        {
            var query = $@"
                SELECT COUNT(1) FROM `{table.Name}`
                WHERE `{table.KeyColumn}` = @KeyValue";

            var count = await connection.ExecuteScalarAsync<int>(query, new { KeyValue = keyValue });
            return count > 0;
        }

        private async Task InsertRecordAsync(MySqlConnection connection, TableConfig table, IDictionary<string, object> row)
        {
            var columns = string.Join(", ", row.Keys.Select(k => $"`{k}`"));
            var values = string.Join(", ", row.Keys.Select((k, i) => $"@p{i}"));

            var query = $@"
                INSERT INTO `{table.Name}` ({columns})
                VALUES ({values})";

            var parameters = new DynamicParameters();
            foreach (var kvp in row)
            {
                parameters.Add($"p{parameters.ParameterNames.Count()}", kvp.Value);
            }
            Console.WriteLine(query);
            await connection.ExecuteAsync(query, parameters);
        }
    }
}
