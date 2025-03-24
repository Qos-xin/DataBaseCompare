# 数据库比对工具

这是一个用于比对两个数据库之间数据差异的工具。主要用于数据迁移后的数据一致性验证。

## 功能特点

- 支持多表数据比对
- 可配置时间范围
- 支持自定义主键和时间字段
- 详细的比对报告输出
- 支持日志表关联查询

## 配置说明

在 `appsettings.json` 中配置以下内容：

### 数据库连接
```json
"ConnectionStrings": {
  "SourceDatabase": "源数据库连接字符串",
  "TargetDatabase": "目标数据库连接字符串"
}
```

### 比对设置
```json
"CompareSettings": {
  "LogTable": {
    "Name": "日志表名",
    "OrderNoColumn": "订单号字段",
    "DateColumn": "时间字段"
  },
  "CompareDateRange": {
    "StartDate": "开始时间",
    "EndDate": "结束时间"
  },
  "Tables": [
    {
      "Name": "表名",
      "KeyColumn": "主键字段",
      "DateColumn": "时间字段"
    }
  ]
}
```

## 使用方法

1. 配置 `appsettings.json` 文件
2. 运行程序：
   ```bash
   dotnet run
   ```
3. 查看比对结果报告

## 注意事项

- 确保数据库连接字符串正确
- 比对时间范围不要过大，建议按天进行比对
- 主键字段必须唯一
- 时间字段必须存在且格式正确

## 开发环境

- .NET 6.0+
- C#
- MySQL

## 许可证

MIT License 