using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TEST.Cache.Data;
using TEST.Cache.Entities;

namespace TEST.Cache.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController : ControllerBase
    {
        private IMemoryCache _cache;
        private readonly ApplicationContext _context;
        public CacheController(IMemoryCache cache, ApplicationContext context)
        {
            _cache = cache;
            _context = context;
        }

        /// <summary>
        /// Gets the entity from DB if not in cache.
        /// </summary>
        /// <returns></returns>
        private async Task<ICollection<DbModel>> AllDbModel()
        {
            var dbModel = new List<DbModel>();
            if (!_cache.TryGetValue(nameof(DbModel), out dbModel))
            {
                if (dbModel == null)
                    dbModel = await _context.DbModel.AsNoTracking().ToListAsync();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(30));
                _cache.Set(nameof(DbModel), dbModel, cacheEntryOptions);
            }
            return dbModel;
        }

        /// <summary>
        /// Gets entity information and shows time required to do so.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetDbModel()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = await AllDbModel();
            stopwatch.Stop();
            return Ok(new { time = stopwatch.Elapsed, data = result });
        }

        /// <summary>
        /// Creates a new entry of an entity in the DB and in cache.
        /// </summary>
        /// <param name="DbModel">Entity objecto to create.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PostDbModel(DbModel entity)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(entity);

                #region Db
                await _context.DbModel.AddAsync(entity);
                await _context.SaveChangesAsync();
                #endregion

                #region Cache to show
                await AllDbModel(); //Ensures the information on cache
                var cacheShowName = nameof(DbModel);
                var dbModelMemoryCache = _cache.Get(cacheShowName) as List<DbModel>;
                if (dbModelMemoryCache != null)
                {
                    dbModelMemoryCache.Add(entity);
                    _cache.Remove(cacheShowName);
                    _cache.Set(cacheShowName, dbModelMemoryCache);
                }
                #endregion

                #region Cache to save
                //var cacheSaveName = $"{nameof(DbModel)}Post";
                //var dbModelMemoryCache2 = _cache.Get(cacheSaveName) as List<DbModel>;
                //if (dbModelMemoryCache2 != null)
                //{
                //    dbModelMemoryCache2.Add(dbModel);
                //    _cache.Remove(cacheSaveName);
                //    _cache.Set(cacheSaveName, dbModelMemoryCache2);
                //}
                //else
                //{
                //    var newCache = new List<DbModel>();
                //    newCache.Add(dbModel);
                //    _cache.Set(cacheSaveName, newCache);
                //}
                //SaveCacheInDB();
                #endregion


                return Ok(entity);
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Edits an entry of an entity in the DB and in cache.
        /// </summary>
        /// <param name="DbModel">Entity objecto to edit.</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> PutDbModel(DbModel entity)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(entity);

                #region Db
                //if (await _context.DbModel.FindAsync(dbModel.Id) == null) 
                //    return NoContent();
                //_context.DbModel.Update(dbModel);
                //await _context.SaveChangesAsync();
                #endregion

                #region Cache to show
                await AllDbModel(); //Ensures the information on cache
                var cacheShowName = nameof(DbModel);
                var dbModelMemoryCache = _cache.Get(cacheShowName) as List<DbModel>;
                if (dbModelMemoryCache != null)
                {
                    var dbModelExists = dbModelMemoryCache.FirstOrDefault(x => x.Id == entity.Id);
                    if (dbModelExists != null)
                    {
                        var oldDbModelRecord = dbModelMemoryCache.FirstOrDefault(x => x.Id == entity.Id);
                        var oldDbModelRecordIndex = dbModelMemoryCache.FindIndex(x => x.Id == entity.Id);
                        dbModelMemoryCache.Remove(oldDbModelRecord);
                        dbModelMemoryCache.Insert(oldDbModelRecordIndex, entity);
                        _cache.Remove(cacheShowName);
                        _cache.Set(cacheShowName, dbModelMemoryCache);
                    }
                }
                #endregion

                _context.DbModel.Update(entity);
                await _context.SaveChangesAsync();

                #region Cache to save
                //var cacheSaveName = $"{nameof(DbModel)}Put";
                //var dbModelMemoryCache2 = _cache.Get(cacheSaveName) as List<DbModel>;
                //if (dbModelMemoryCache2 != null)
                //{
                //    var dbModelExists = dbModelMemoryCache.FirstOrDefault(x => x.Id == dbModel.Id);
                //    if (dbModelExists != null)
                //    {
                //        var oldDbModelRecord2 = dbModelMemoryCache2.FirstOrDefault(x => x.Id == dbModel.Id);
                //        var oldDbModelRecordIndex2 = dbModelMemoryCache2.FindIndex(x => x.Id == dbModel.Id);
                //        dbModelMemoryCache2.Remove(oldDbModelRecord2);
                //        dbModelMemoryCache2.Insert(oldDbModelRecordIndex2, dbModel);
                //        _cache.Remove(cacheSaveName);
                //        _cache.Set(cacheSaveName, dbModelMemoryCache2);
                //    }
                //}
                //else
                //{
                //    var newCache = new List<DbModel>();
                //    newCache.Add(dbModel);
                //    _cache.Set(cacheSaveName, newCache);
                //}
                //SaveCacheInDB();
                #endregion

                return Ok(entity);
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Deletes an entry of an entity in the DB and in cache.
        /// </summary>
        /// <param name="id">Id of entity to delete.</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteDbModel(long id)
        {
            try
            {
                if (id == 0)
                    return NoContent();

                #region Db
                //var dbModel = await _context.DbModel.FindAsync(id);
                //if (dbModel == null) 
                //    return NoContent();
                //_context.Remove(dbModel);
                //await _context.SaveChangesAsync();
                #endregion

                #region Cache
                await AllDbModel(); //Ensures the information on cache
                var cacheShowName = nameof(DbModel);
                var dbModelMemoryCache = _cache.Get(cacheShowName) as List<DbModel>;
                if (dbModelMemoryCache != null)
                {
                    var dbModel = dbModelMemoryCache.FirstOrDefault(x => x.Id == id);
                    if (dbModel != null)
                    {
                        var oldDbModelRecord = dbModelMemoryCache.FirstOrDefault(x => x.Id == dbModel.Id);
                        dbModelMemoryCache.Remove(oldDbModelRecord);
                        _cache.Remove(cacheShowName);
                        _cache.Set(cacheShowName, dbModelMemoryCache);
                    }
                }
                #endregion

                var entity = await _context.DbModel.FindAsync(id);
                _context.DbModel.Remove(entity);
                await _context.SaveChangesAsync();

                #region Cache to save
                //var cacheSaveName = $"{nameof(DbModel)}Delete";
                //var dbModelMemoryCache2 = _cache.Get(cacheSaveName) as List<DbModel>;
                //if (dbModelMemoryCache2 != null)
                //{
                //    var dbModel = dbModelMemoryCache.FirstOrDefault(x => x.Id == id);
                //    if (dbModel != null)
                //    {
                //        var oldDbModelRecord2 = dbModelMemoryCache2.FirstOrDefault(x => x.Id == dbModel.Id);
                //        dbModelMemoryCache2.Remove(oldDbModelRecord2);
                //        _cache.Remove(cacheSaveName);
                //        _cache.Set(cacheSaveName, dbModelMemoryCache2);
                //    }
                //}
                //else
                //{
                //    var dbModel = dbModelMemoryCache.FirstOrDefault(x => x.Id == id);
                //    if (dbModel != null)
                //    {
                //        var newCache = new List<DbModel>();
                //        newCache.Add(dbModel);
                //        _cache.Set(cacheSaveName, newCache);
                //    }
                //}
                //SaveCacheInDB();
                #endregion


                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }

        ///// <summary>
        ///// This method saves the changes in cache to the database.
        ///// </summary>
        ///// <returns></returns>
        //[HttpGet("save-cache")]
        //public async Task<IActionResult> SaveCache()
        //{
        //    try
        //    {
        //        SaveCacheInDB();

        //        await AllDbModel(); //Ensures the information on cache

        //        return NoContent();
        //    }
        //    catch (Exception e)
        //    {
        //        return StatusCode(500);
        //    }
        //}

        ///// <summary>
        ///// Internal method to save cache in DB.
        ///// </summary>
        //private async void SaveCacheInDB()
        //{
        //    try
        //    {
        //        //TODO: Lock cache entities
        //        //#region Post
        //        //var cachePostName = $"{nameof(DbModel)}Post";
        //        //var dbModelMemoryCachePost = _cache.Get(cachePostName) as List<DbModel>;
        //        //if (dbModelMemoryCachePost != null)
        //        //{
        //        //    _cache.Remove(cachePostName);
        //        //    await _context.DbModel.AddRangeAsync(dbModelMemoryCachePost);
        //        //}
        //        //#endregion

        //        //#region Put
        //        //var cachePutName = $"{nameof(DbModel)}Put";
        //        //var dbModelMemoryCachePut = _cache.Get(cachePutName) as List<DbModel>;
        //        //if (dbModelMemoryCachePut != null)
        //        //{
        //        //    _cache.Remove(cachePutName);
        //        //    _context.DbModel.UpdateRange(dbModelMemoryCachePut);
        //        //}
        //        //#endregion

        //        #region Delete
        //        //var cacheDeleteName = $"{nameof(DbModel)}Delete";
        //        //var dbModelMemoryCacheDelete = _cache.Get(cacheDeleteName) as List<DbModel>;
        //        //if (dbModelMemoryCacheDelete != null)
        //        //{
        //        //    _cache.Remove(cacheDeleteName);
        //        //    _context.DbModel.RemoveRange(dbModelMemoryCacheDelete);
        //        //}
        //        //#endregion

        //        await _context.SaveChangesAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        throw;
        //    }
        //}

    }
}
