using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace TrainSchedule.Database
{
    public class Db : IDb
    {
        private readonly ILogger<Db> _logger;
        private readonly string dbPath;
        static private readonly char[] invalidFilenameCharacters = Path.GetInvalidFileNameChars();
        static private ConcurrentDictionary<string, int> dblocks = new();
        const string extension = "dbr";

        public Db(ILogger<Db> logger)
        {
            _logger = logger;

            dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db");
            Console.WriteLine(dbPath);
            if (!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath);
            }
        }

        private static string MakeSafeFilename(string name)
        {
            return string.Join("", name.ToCharArray().Select(c => invalidFilenameCharacters.Contains(c) ? '_' : c)) + "." + extension;
        }

        private static void Lock(string key)
        {
            var count = dblocks.AddOrUpdate(key, k => 1, (k, current) => current + 1);
            if (count > 1)
                while (dblocks[key] > 1)
                    Task.Delay(250);
        }

        private static void Unlock(string key)
        {
            dblocks.AddOrUpdate(key, k => 0, (k, current) => current - 1);
        }

        private async Task<int> Write(string key, byte[] data)
        {
            var safeKey = MakeSafeFilename(key);
            var filepath = Path.Combine(dbPath, safeKey);
            try
            {
                Lock(safeKey);
                if (data.Length == 0)
                {
                    if (File.Exists(safeKey))
                    {
                        File.Delete(safeKey);
                    }
                    return 0;
                }
                await File.WriteAllBytesAsync(filepath, data);
                return data.Length;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Write Error");
                throw;
            }
            finally
            {
                Unlock(safeKey);
            }
        }

        public async Task<int> Write(string key, string body)
        {
            return await Write(key, Encoding.UTF8.GetBytes(body));
        }

        public async Task<byte[]> Read(string key)
        {
            await Task.Delay(0);
            var safeKey = MakeSafeFilename(key);
            var filepath = Path.Combine(dbPath, safeKey);
            try
            {
                Lock(safeKey);
                if (File.Exists(filepath))
                {
                    return await File.ReadAllBytesAsync(filepath);
                }
                throw new KeyNotFoundException($"{key}/{safeKey} was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Read Error");
                throw;
            }
            finally
            {
                Unlock(safeKey);
            }
        }

        public async Task<IEnumerable<string>> GetKeys()
        {
            await Task.Delay(0);
            return (from file in
                    (from item in Directory.EnumerateFiles(dbPath, "*." + extension)
                     select new DirectoryInfo(item))
                    select file.Name[0..^file.Extension.Length]).ToArray();
        }

        public async Task Delete(string key)
        {
            await Task.Delay(0);
            var safeKey = MakeSafeFilename(key);
            try
            {
                Lock(safeKey);
                if (File.Exists(safeKey))
                {
                    File.Delete(safeKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete Error");
                throw;
            }
            finally
            {
                Unlock(safeKey);
            }
        }

    }
}
