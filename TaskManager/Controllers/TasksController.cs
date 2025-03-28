﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;
using Task = TaskManager.Models.Task;
using StackExchange.Redis;
using System.Text.Json;

namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IDatabase _redisDb;

        public TasksController(ApplicationDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redisDb = redis.GetDatabase();
        }

        /// <summary>
        /// 取得所有任務
        /// </summary>
        /// <returns>任務列表</returns>
        /// <response code="200">成功回傳任務列表</response>
        // GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Task>>> GetTasks()
        {
            string cacheKey = "tasks";
            string cachedTasks = await _redisDb.StringGetAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedTasks))
            {
                var tasks = JsonSerializer.Deserialize<List<Task>>(cachedTasks);
                return Ok(tasks);
            }

            var taskList = await _context.Tasks.ToListAsync();
            await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(taskList), TimeSpan.FromMinutes(5));

            return Ok(taskList);
            //return await _context.Tasks.ToListAsync();
        }

        /// <summary>
        /// 取得指定 ID 的任務
        /// </summary>
        /// <param name="id">任務 ID</param>
        /// <returns>指定的任務</returns>
        /// <response code="200">成功回傳任務</response>
        /// <response code="404">找不到該任務</response>
        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Task>> GetTask(int id)
        {
            string cacheKey = $"task:{id}";
            string cachedTask = await _redisDb.StringGetAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedTask))
            {
                var task = JsonSerializer.Deserialize<Task>(cachedTask);
                return Ok(task);
            }

            var taskFromDb = await _context.Tasks.FindAsync(id);

            if (taskFromDb == null)
            {
                return NotFound();
            }

            await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(taskFromDb), TimeSpan.FromMinutes(5));

            return Ok(taskFromDb);
        }

        /// <summary>
        /// 更新指定 ID 的任務
        /// </summary>
        /// <param name="id">任務 ID</param>
        /// <param name="task">更新後的任務內容</param>
        /// <returns>無內容</returns>
        /// <response code="204">成功更新</response>
        /// <response code="400">請求格式錯誤</response>
        /// <response code="404">找不到該任務</response>
        // PUT: api/Tasks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask(int id, Task task)
        {
            if (id != task.Id)
            {
                return BadRequest();
            }

            var existingTask = await _context.Tasks.FindAsync(id);
            if (existingTask == null)
            {
                return NotFound();
            }

            // 允許更新 IsCompleted 屬性
            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.IsCompleted = task.IsCompleted;

            _context.Entry(existingTask).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await _redisDb.KeyDeleteAsync($"task:{id}"); // 刪除 Redis 快取
                await _redisDb.KeyDeleteAsync("tasks"); // 讓列表快取失效
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tasks.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// 建立新的任務
        /// </summary>
        /// <param name="task">要建立的任務物件</param>
        /// <returns>回傳新建的任務</returns>
        /// <response code="201">成功建立任務</response>
        /// <response code="400">請求格式錯誤</response>
        // POST: api/Tasks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Task>> PostTask(Task task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            await _redisDb.KeyDeleteAsync("tasks");

            return CreatedAtAction("GetTask", new { id = task.Id }, task);
        }

        /// <summary>
        /// 刪除指定 ID 的任務
        /// </summary>
        /// <param name="id">任務 ID</param>
        /// <returns>無內容</returns>
        /// <response code="204">成功刪除</response>
        /// <response code="404">找不到該任務</response>
        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            // 刪除 Redis 快取 (單個任務 + 全部任務)
            await _redisDb.KeyDeleteAsync($"task:{id}");
            await _redisDb.KeyDeleteAsync("tasks");

            return NoContent();
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
