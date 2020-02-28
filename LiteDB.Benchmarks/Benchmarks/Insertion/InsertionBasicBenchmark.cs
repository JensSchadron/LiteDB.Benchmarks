using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using LiteDB.Benchmarks.Benchmarks.Base;
using LiteDB.Benchmarks.Models;
using LiteDB.Benchmarks.Models.Generators;

namespace LiteDB.Benchmarks.Benchmarks.Insertion
{
    [BenchmarkCategory(Constants.Categories.INSERTION)]
    public class InsertionBasicBenchmark : DatabaseBenchmarkBase
    {
        protected override string DatabasePath => Constants.DatabaseNames.INSERTION;

        private List<FileMetaBase> data;

        private LiteDatabase DatabaseInstance { get; set; }
        private LiteCollection<FileMetaBase> _fileMetaCollection { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            File.Delete(DatabasePath);

            DatabaseInstance = new LiteDatabase(ConnectionString);
            _fileMetaCollection = DatabaseInstance.GetCollection<FileMetaBase>();

            data = FileMetaGenerator<FileMetaBase>.GenerateList(N); // executed once per each N value
        }
        
        [IterationSetup]
        

        [Benchmark(Baseline = true)]
        public int Insertion()
        {
            var count = _fileMetaCollection.Insert(data);
            DatabaseInstance.Checkpoint();
            return count;
        }

        [Benchmark]
        public void InsertionWithLoop()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < data.Count; i++)
            {
                _fileMetaCollection.Insert(data[i]);
            }
            DatabaseInstance.Checkpoint();
        }

        [Benchmark]
        public int Upsertion()
        {
            var count = _fileMetaCollection.Upsert(data);
            DatabaseInstance.Checkpoint();
            return count;
        }

        [Benchmark]
        public void UpsertionWithLoop()
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < data.Count; i++)
            {
                _fileMetaCollection.Upsert(data[i]);
            }
            DatabaseInstance.Checkpoint();
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            const string collectionName = nameof(FileMetaBase);

            DatabaseInstance.DropCollection(collectionName);

            DatabaseInstance.Checkpoint();
            DatabaseInstance.Rebuild();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            DatabaseInstance.DropCollection(nameof(FileMetaBase));
            DatabaseInstance.Checkpoint();
            DatabaseInstance.Dispose();

            File.Delete(DatabasePath);
        }
    }
}