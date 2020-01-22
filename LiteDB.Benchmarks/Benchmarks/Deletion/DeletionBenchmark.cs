using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using LiteDB.Benchmarks.Benchmarks.Base;
using LiteDB.Benchmarks.Models;
using LiteDB.Benchmarks.Models.Generators;

namespace LiteDB.Benchmarks.Benchmarks.Deletion
{
    [BenchmarkCategory(Constants.Categories.DELETION)]
    public class DeletionBenchmark : DatabaseBenchmarkBase
    {
        protected override string DatabasePath => Constants.DatabaseNames.DELETION;

        private List<FileMetaBase> data;

        private LiteDatabase DatabaseInstance { get; set; }
        private LiteCollection<FileMetaBase> _fileMetaCollection { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            File.Delete(DatabasePath);

            DatabaseInstance = new LiteDatabase(ConnectionString);
            _fileMetaCollection = DatabaseInstance.GetCollection<FileMetaBase>();
            _fileMetaCollection.EnsureIndex(file => file.IsFavorite);
            _fileMetaCollection.EnsureIndex(file => file.ShouldBeShown);

            data = FileMetaGenerator<FileMetaBase>.GenerateList(N);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _fileMetaCollection.Insert(data);
        }

        [Benchmark(Baseline = true)]
        public int DeleteAllExpression()
        {
            return _fileMetaCollection.DeleteMany("1 = 1");
        }

        [Benchmark]
        public void DropCollectionAndRecreate()
        {
            const string collectionName = nameof(FileMetaBase);

            var indexesCollection = DatabaseInstance.GetCollection("$indexes");
            var droppedCollectionIndexes = indexesCollection.Query().Where(x => x["collection"] == collectionName && x["name"] != "_id").ToDocuments().ToList();

            DatabaseInstance.DropCollection(collectionName);

            foreach (var indexInfo in droppedCollectionIndexes)
            {
                DatabaseInstance.GetCollection(collectionName).EnsureIndex(indexInfo["name"], BsonExpression.Create(indexInfo["expression"]), indexInfo["unique"]);
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            // Disposing logic
            DatabaseInstance.DropCollection(nameof(FileMetaBase));
            DatabaseInstance.Dispose();

            File.Delete(DatabasePath);
        }
    }
}