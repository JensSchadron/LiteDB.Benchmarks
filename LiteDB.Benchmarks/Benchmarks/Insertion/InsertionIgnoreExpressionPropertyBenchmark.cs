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
    public class InsertionIgnoreExpressionPropertyBenchmark : DatabaseBenchmarkBase
    {
        protected override string DatabasePath => Constants.DatabaseNames.INSERTION;

        private List<FileMetaBase> baseData;
        private List<FileMetaWithExclusion> baseDataWithBsonIgnore;

        private LiteDatabase DatabaseInstance { get; set; }
        private LiteCollection<FileMetaBase> _fileMetaCollection { get; set; }
        private LiteCollection<FileMetaWithExclusion> _fileMetaExclusionCollection { get; set; }

        [GlobalSetup(Target = nameof(Insertion))]
        public void GlobalBsonIgnoreSetup()
        {
            File.Delete(DatabasePath);

            DatabaseInstance = new LiteDatabase(ConnectionString);
            _fileMetaCollection = DatabaseInstance.GetCollection<FileMetaBase>();
            _fileMetaCollection.EnsureIndex(fileMeta => fileMeta.ShouldBeShown);

            baseData = FileMetaGenerator<FileMetaBase>.GenerateList(N); // executed once per each N value
        }

        [GlobalSetup(Target = nameof(InsertionWithBsonIgnore))]
        public void GlobalIgnorePropertySetup()
        {
            File.Delete(DatabasePath);

            DatabaseInstance = new LiteDatabase(ConnectionString);
            _fileMetaExclusionCollection = DatabaseInstance.GetCollection<FileMetaWithExclusion>();
            _fileMetaExclusionCollection.EnsureIndex(fileMeta => fileMeta.ShouldBeShown);

            baseDataWithBsonIgnore = FileMetaGenerator<FileMetaWithExclusion>.GenerateList(N); // executed once per each N value
        }

        [Benchmark(Baseline = true)]
        public int Insertion()
        {
            return _fileMetaCollection.Insert(baseData);
        }

        [Benchmark]
        public int InsertionWithBsonIgnore()
        {
            return _fileMetaExclusionCollection.Insert(baseDataWithBsonIgnore);
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            var indexesCollection = DatabaseInstance.GetCollection("$indexes");
            var droppedCollectionIndexes = indexesCollection.Query().Where(x => x["name"] != "_id").ToDocuments().ToList();

            var collectionNames = DatabaseInstance.GetCollectionNames();
            foreach (var name in collectionNames)
            {
                DatabaseInstance.DropCollection(name);
            }

            foreach (var indexInfo in droppedCollectionIndexes)
            {
                DatabaseInstance.GetCollection(indexInfo["collection"]).EnsureIndex(indexInfo["name"], BsonExpression.Create(indexInfo["expression"]), indexInfo["unique"]);
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            baseData?.Clear();
            baseData = null;

            baseDataWithBsonIgnore?.Clear();
            baseDataWithBsonIgnore = null;

            DatabaseInstance.DropCollection(nameof(FileMetaBase));
            DatabaseInstance.DropCollection(nameof(FileMetaWithExclusion));
            DatabaseInstance.Dispose();

            File.Delete(DatabasePath);
        }
    }
}