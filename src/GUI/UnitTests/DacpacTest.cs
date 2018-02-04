﻿using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.SqlServer.Dac.Extensions.Prototype;
using NUnit.Framework;
using ReverseEngineer20;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests
{
    [TestFixture]
    public class DacpacTest
    {
        private const string  dacpac = "Chinook.dacpac";

        private const string dacpacQuirk = "TestDb.dacpac";

        private static readonly ISet<string> _dateTimePrecisionTypes = new HashSet<string> { "datetimeoffset", "datetime2", "time" };

        private static readonly ISet<string> _maxLengthRequiredTypes
            = new HashSet<string> { "binary", "varbinary", "char", "varchar", "nchar", "nvarchar" };

        private TSqlTypedModel model;
        [SetUp]
        public void Setup()
        {
            model = new TSqlTypedModel(dacpac);
        }

        [Test]
        public void CanGetTableNames()
        {
            // Arrange
            var builder = new DacpacTableListBuilder(dacpac);

            // Act
            var result = builder.GetTableNames();

            // Assert
            Assert.AreEqual("dbo.Album", result[0]);
            Assert.AreEqual(11, result.Count);
        }

        [Test]
        public void CanEnumerateTables()
        {
            // Arrange
            var factory = new SqlServerDacpacDatabaseModelFactory(null);

            // Act
            var dbModel = factory.Create(dacpac, new List<string>(), new List<string>());

            // Assert
            Assert.AreEqual(0, dbModel.Tables.Count());
        }

        [Test]
        public void CanEnumerateSelectedTables()
        {
            // Arrange
            var factory = new SqlServerDacpacDatabaseModelFactory(null);
            var tables = new List<string> { "dbo.Album", "dbo.Artist", "dbo.InvoiceLine" };

            // Act
            var dbModel = factory.Create(dacpac, tables, new List<string>());

            // Assert
            Assert.AreEqual(3, dbModel.Tables.Count());
            Assert.AreEqual("Album", dbModel.Tables[0].Name);
            Assert.AreEqual(1, dbModel.Tables[0].ForeignKeys.Count);
            Assert.AreEqual(3, dbModel.Tables[0].Columns.Count);
        }

        [Test]
        public void CanEnumerateSelectedQuirkObjects()
        {
            // Arrange
            var factory = new SqlServerDacpacDatabaseModelFactory(null);
            var tables = new List<string> { "dbo.FilteredIndexTable", "dbo.DefaultComputedValues" };

            // Act
            var dbModel = factory.Create(dacpacQuirk, tables, new List<string>());

            // Assert
            Assert.AreEqual(2, dbModel.Tables.Count());

            Assert.AreEqual("FilteredIndexTable", dbModel.Tables[1].Name);
            Assert.AreEqual(0, dbModel.Tables[1].ForeignKeys.Count);
            Assert.AreEqual(2, dbModel.Tables[1].Columns.Count);

            //TODO Add support for computed columns (expect 7!)
            Assert.AreEqual("DefaultComputedValues", dbModel.Tables[0].Name);
            Assert.AreEqual(5, dbModel.Tables[0].Columns.Count);
        }

        [Test]
        public void CanEnumerateTypeAlias()
        {
            // Arrange
            var factory = new SqlServerDacpacDatabaseModelFactory(null);
            var tables = new List<string> { "dbo.TypeAlias" };

            // Act
            var dbModel = factory.Create(dacpacQuirk, tables, new List<string>());

            // Assert
            Assert.AreEqual(1, dbModel.Tables.Count());

            Assert.AreEqual("TypeAlias", dbModel.Tables[0].Name);
            Assert.AreEqual(2, dbModel.Tables[0].Columns.Count);

            Assert.AreEqual("TestTypeAlias", dbModel.Tables[0].Columns[1].StoreType);
            Assert.AreEqual("nvarchar(max)", dbModel.Tables[0].Columns[1].GetUnderlyingStoreType());
        }
    }
}