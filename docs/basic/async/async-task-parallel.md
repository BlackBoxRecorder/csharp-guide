---
title: 异步、多线程与并行
description: 理清 C# 中异步、多线程和并行三者的本质区别，掌握决策树和典型使用场景，避免常见的混用陷阱。
---

# 异步、多线程与并行

异步、多线程、并行——这三个概念在 .NET 中经常被混用，但它们的**动机**完全不同。搞清楚它们各自解决什么问题，才能在正确的场景选择正确的方案。

> **前置阅读**：[什么是异步](/basic/async/async) - 理解异步的本质

## 本质区分

| | 一句话 | 动机 | .NET 里的典型写法 |
|---|---|---|---|
| **异步** | 等在别处，线程回家 | **省线程** —— 等待期间不占资源 | `await` + `XxxAsync` |
| **多线程** | 多雇几个人 | **不阻塞/隔离** —— 让调用方脱身 | `Task.Run`、`Thread`、`BackgroundService` |
| **并行** | 多雇几个人同时干一件大活 | **缩短墙钟时间** —— 吃满多核 | `Parallel.For`、PLINQ、`Task.Run` 多份 |

### 多线程与并行的关系

**多线程是"手段"，并行是"目的"。**

- 并行**通常通过**多线程实现（`Parallel.For` 和 `Task.Run` 底层都是线程池线程）
- 但**多线程 ≠ 并行**：单核机器上多线程只是时间片轮转的"并发"，并没有同时计算
- 且**并行 ≠ 多线程**：SIMD（`Vector<T>`）、GPU、多进程也是并行

顺带区分两个常被混用的词：
- **并发 (Concurrency)** = 同时**应对**多件事（是代码结构）
- **并行 (Parallelism)** = 同时**做**多件事（是执行状态）

**异步实现高并发，多线程实现并行。** 这句话值得记住。

## 决策树：三问定方案

```
                  这个任务在「等」还是在「算」？
                              │
        ┌─────────────────────┴─────────────────────┐
        │                                           │
      【等】                                      【算】
   网络/数据库/磁盘/定时器/                     消耗 CPU：加解密、
   下游服务/用户响应                            图像处理/压缩/推理/大批量计算
        │                                           │
        ▼                          ┌────────────────┼────────────────┐
  ✅ 异步 async/await               │                │                │
     用 XxxAsync API         想缩短总耗时？     只是不想        需要长驻/隔离/
     （等待期零线程占用）          │            卡住调用方？     独立生命周期？
                                  │                │                │
                            任务能拆分吗？      ✅ 多线程        ✅ 专用线程
                           ┌──────┴──────┐      Task.Run        Thread /
                         能              不能     （单个）         BackgroundService
                          │                │
                    ✅ 并行           ❌ 没法并行
                  Parallel.For        （只能优化算法
                  PLINQ               或换更快的库）
                  Task.Run 多份
                  （上限 ≈ 核心数）
```

## 什么时候用异步

### 判据

**等待期间，CPU 是不是闲着？** 是 → 用异步。

具体信号：
- 操作涉及**网络**（HTTP、gRPC、Redis、消息队列、数据库）
- 操作涉及**磁盘**、**管道**、**Socket**
- 单纯的时间等待（`Task.Delay`）
- 需要**同时处理大量这样的等待**（几十到几万）

### 典型场景

```csharp
// ✅ Web API：请求处理全程异步，用少量线程扛高并发
app.MapGet("/orders/{id}", async (int id, AppDbContext db, ICache cache) =>
{
    var cached = await cache.GetAsync($"order:{id}");   // Redis
    if (cached is not null) return Results.Ok(cached);

    var order = await db.Orders.FindAsync(id);          // 数据库
    await cache.SetAsync($"order:{id}", order);
    return Results.Ok(order);
});

// ✅ 批量并发：1000 个请求，峰值只用几个线程
var tasks = urls.Select(u => http.GetStringAsync(u));
var pages = await Task.WhenAll(tasks);

// ✅ 扇出聚合：几个下游服务同时调，总耗时 = 最慢的那个，而非累加
var (user, orders, coupons) = await (
    userSvc.GetAsync(id),
    orderSvc.ListAsync(id),
    couponSvc.ListAsync(id));
```

### 异步的硬前提

**必须有真正的异步 API。** 这是铁律：

```csharp
await File.ReadAllTextAsync(path);        // ✅
await db.Orders.ToListAsync();            // ✅ (EF Core)
await connection.QueryAsync<T>(sql);      // ✅ (Dapper)
await Task.Delay(1000);                   // ✅

await Task.Run(() => File.ReadAllText(path));  // ❌ 假异步
Task.Delay(1000).Wait();                       // ❌ 等于 Thread.Sleep
```

## 什么时候用多线程

### 判据

**不是为了"更快"，而是为了"不占着"或"隔离"。**

### 四种典型场景

**1. 只有同步 API 的遗留库（无奈但合理）**

```csharp
// 这个 SDK 只提供同步方法，没得选
var report = await Task.Run(() => legacySdk.GenerateReport(param));
```

注意：这是**止血**，不解决吞吐量——阻塞只是被转移到了线程池。

**2. 不想卡住调用方（UI / 请求线程）**

```csharp
// UI：不冻结界面
private async void Btn_Click(object s, EventArgs e)
{
    var result = await Task.Run(() => HeavyImageProcessing(bitmap));
    pictureBox.Image = result;   // await 后自动回到 UI 线程
}

// Web：CPU 密集的活挪走，释放请求线程
app.MapGet("/export", () => Task.Run(() => BuildExcel()));
```

**3. 需要独立生命周期**

长驻后台、轮询、看门狗、生产者-消费者：

```csharp
public class PollingWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await DoWorkAsync(ct);
            await Task.Delay(TimeSpan.FromMinutes(1), ct);  // 异步等待，不占线程
        }
    }
}
```

**4. 需要特殊线程模型**

COM 互操作、某些 WPF/WinForms 组件要求 **STA 线程**，必须用 `Thread` 显式指定：

```csharp
var t = new Thread(() => { /* COM 调用 */ });
t.SetApartmentState(ApartmentState.STA);
t.Start();
```

### Task.Run 适用边界速查

| 场景 | 该不该用 | 理由 |
|---|---|---|
| CPU 密集，UI 应用 | ✅ 用 | 不冻结界面，`await` 后自动回到 UI 线程 |
| CPU 密集，Web 应用 | ✅ 用 | 释放请求线程，且多核并行 |
| 只有同步 API 的遗留库 | ⚠️ 无奈用 | 比阻塞请求线程好，但属于止血，不解决吞吐 |
| 包 `File.ReadAllText` | ❌ 禁用 | 用 `ReadAllTextAsync` |
| 包 `connection.ExecuteReader` | ❌ 禁用 | 用 `ExecuteReaderAsync` |
| 包 `Thread.Sleep` | ❌ 禁用 | 用 `await Task.Delay` |

## 什么时候用并行

### 判据

三个条件**同时满足**才值得：

1. **CPU 密集**（在算，不是等）
2. **可拆分**（数据或任务之间没有强依赖）
3. **量足够大**（拆分开销 < 并行收益）

### 典型场景

```csharp
// 图像批处理：每张图独立，天然可并行
Parallel.ForEach(images, img =>
{
    var resized = Resize(img, 800, 600);
    File.WriteAllBytes(GetOutputPath(img), Encode(resized));
});

// PLINQ：数据并行 + 聚合
var total = bigDataset
    .AsParallel()
    .Where(x => x.IsValid)
    .Select(x => ComputeScore(x))
    .Sum();

// 手动分块：需要控制粒度时
var chunks = data.Chunk(data.Length / Environment.ProcessorCount);
var results = await Task.WhenAll(chunks.Select(c => Task.Run(() => ProcessChunk(c))));
var merged = results.SelectMany(r => r).ToList();
```

### 关键的"不值得"边界

```csharp
// ❌ 数据量太小 —— 拆分+调度+合并的开销 > 省下的时间
Parallel.For(0, 10, i => list[i]++);

// ❌ 并行里混了 I/O —— 该用异步，不该占着线程等
Parallel.ForEach(urls, url => httpClient.Send(new HttpRequestMessage(HttpMethod.Get, url)));
//  ↑ 正确写法：await Task.WhenAll(urls.Select(u => http.GetStringAsync(u)))

// ❌ 有共享状态且没加锁 —— 结果错乱
int sum = 0;
Parallel.For(0, 1_000_000, i => sum += i);   // 竞态，结果错误
//  ↑ 正确写法：用 Interlocked 或 PLINQ 的 Sum()
```

### 加速上限

实测数据（3 核机器）：

| 并行度 | 加速比 |
|---|---|
| 串行 | 1.00x |
| 2 线程 | 2.02x |
| **3 线程** | **2.35x** ← 天花板 |
| 4 线程 | 1.78x ← 回落 |
| 12 线程 | 1.82x |

**超过核心数就不再加速，反而因上下文切换和缓存失效而变慢。** Amdahl 定律：`加速上限 = 1 / (串行部分 + 并行部分 / 核心数)`。

## 真实项目：三者混合使用

不要把它们当成互斥选项。看一个典型的数据管道：

```csharp
public async Task<Report> BuildReportAsync(ReportRequest req)
{
    // ① 异步：并发拉取多个数据源（等待期零线程占用）
    var (rawA, rawB, rawC) = await (
        FetchFromApiA(req),
        FetchFromApiB(req),
        db.QueryAsync(req));

    // ② 并行：CPU 密集的数据清洗，吃满多核缩短时间
    //    再套一层 Task.Run，避免占死请求线程
    var cleaned = await Task.Run(() =>
        new[] { rawA, rawB, rawC }
            .AsParallel()
            .SelectMany(x => CleanAndNormalize(x))
            .ToList());

    // ③ 异步：写回存储
    await storage.SaveAsync(cleaned);
    await cache.SetAsync(req.Key, cleaned);

    return Report.From(cleaned);
}
```

**分工非常清晰：等的地方用异步，算的地方用并行，整体用 `Task.Run` 包住不让请求线程被拖住。**

## 代价与常见坑

| | 代价 | 常见坑 |
|---|---|---|
| **异步** | 状态机分配、上下文捕获、调试栈变复杂 | `async void`（异常无法捕获）<br>`.Result`/`.Wait()` 导致死锁（有 `SynchronizationContext` 时）<br>忘记 `ConfigureAwait(false)`（库代码里）<br>拿异步去做 CPU 密集（毫无收益） |
| **多线程** | 1MB/线程栈、上下文切换冲刷缓存、GC 要扫所有栈 | 线程池饥饿（阻塞 + 慢速注入）<br>锁竞争、死锁<br>线程数开太多 |
| **并行** | 拆分/合并开销、内存带宽争抢、GC 压力 | 小数据集强行并行（反而更慢）<br>并行里做 I/O<br>共享状态竞态<br>以为线程越多越快 |

### 一个高频反模式

```csharp
// ❌ 三重错误：异步 + 并行 + 阻塞 混用
var results = urls
    .AsParallel()                                    // 用并行做 I/O（错）
    .Select(u => httpClient.Send(new HttpRequestMessage(HttpMethod.Get, u)))  // 同步 API（错）
    .ToList();

// ✅ 正确：纯异步，线程占用恒定
var results = await Task.WhenAll(urls.Select(u => http.GetStringAsync(u)));
```

## 速查口诀

> **等在别处用异步，算得慢了用并行；
> 只是不想卡住人，一个 `Task.Run` 就够用。**

遇到纠结时自问：

1. **等待时 CPU 闲着吗？** 闲 → 异步
2. **想让总时间变短吗？** 想，且是 CPU 活 → 并行
3. **只是不想阻塞调用方？** 是 → `Task.Run` 单个

**异步提升的是吞吐量（同样的资源扛更多请求），并行提升的是速度（单个大任务算得更快）。** 别指望异步让一个 10 秒的计算变成 1 秒——那是并行的活。

**下一步学习**：
- [异步编程基础](/basic/async/async-basics) - async/await、Task、ValueTask 核心概念
- [异步编程模式](/basic/async/async-patterns) - 并行执行、取消、进度报告等
