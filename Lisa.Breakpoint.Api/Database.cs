﻿using Lisa.Common.TableStorage;
using Lisa.Common.WebApi;
using Microsoft.Extensions.OptionsModel;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lisa.Breakpoint.Api
{
    public class Database
    {
        public Database(IOptions<TableStorageSettings> settings)
        {
            _settings = settings.Value;
        }
        
        public async Task<IEnumerable<DynamicModel>> FetchReports(List<Tuple<string, string>> filter)
        {
            CloudTable table = await Connect("Reports");

            var query = new TableQuery<DynamicEntity>();

            if (filter.Count > 0)
            {
                var filterCondition = CreateFilter(filter);
                query = query.Where(filterCondition);
            }

            var reports = await table.ExecuteQuerySegmentedAsync(query, null);
            var results = reports.Select(r => ReportMapper.ToModel(r));

            return results;
        }

        public async Task<DynamicModel> FetchReport(Guid id)
        {
            CloudTable table = await Connect("Reports");

            var query = new TableQuery<DynamicEntity>().Where(TableQuery.GenerateFilterConditionForGuid("id", QueryComparisons.Equal, id));
            var report = await table.ExecuteQuerySegmentedAsync(query, null);
            var result = report.Select(r => ReportMapper.ToModel(r)).SingleOrDefault();

            return result;
        }

        public async Task<IEnumerable<DynamicModel>> FetchMemberships(string projectName)
        {
            CloudTable table = await Connect("Memberships");

            var query = new TableQuery<DynamicEntity>().Where(TableQuery.GenerateFilterCondition("project", QueryComparisons.Equal, projectName));
            var membership = await table.ExecuteQuerySegmentedAsync(query, null);
            var result = membership.Select(m => MemberShipsMapper.ToModel(m));

            return result;
        }

        public async Task<IEnumerable<DynamicModel>> FetchComments(Guid id)
        {
            CloudTable table = await Connect("Comments");

            string newId = id.ToString();
            var query = new TableQuery<DynamicEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, newId));
            var comments = await table.ExecuteQuerySegmentedAsync(query, null);
            var result = comments.Select(c => CommentMapper.ToModel(c));
            var sortedComments = ReportSorter.Sort(result, "datetime", "asc");

            return sortedComments;
        }

        public async Task<DynamicModel> SaveReport(dynamic report)
        {
            CloudTable table = await Connect("Reports");

            dynamic reportEntity = ReportMapper.ToEntity(report);

            reportEntity.PartitionKey = reportEntity.project;
            reportEntity.RowKey = reportEntity.id.ToString();

            var InsertOperation = TableOperation.Insert(reportEntity);
            await table.ExecuteAsync(InsertOperation);
            var result = ReportMapper.ToModel(reportEntity);

            return result;
        }
        
        public async Task<DynamicModel> SaveComment(dynamic comment, Guid id)
        {
            CloudTable table = await Connect("Comments");

            dynamic commentEntity = CommentMapper.ToEntity(comment);

            commentEntity.PartitionKey = id.ToString();
            commentEntity.RowKey = commentEntity.id.ToString();

            var InsertOperation = TableOperation.Insert(commentEntity);
            await table.ExecuteAsync(InsertOperation);
            var result = CommentMapper.ToModel(commentEntity);

            return result;
        }

        public async Task<DynamicModel> SaveMembership(dynamic membership)
        {
            CloudTable table = await Connect("Memberships");

            dynamic membershipEntity = MemberShipsMapper.ToEntity(membership);

            membershipEntity.PartitionKey = membershipEntity.project;
            membershipEntity.RowKey = membershipEntity.id.ToString();

            var InsertOperation = TableOperation.Insert(membershipEntity);
            await table.ExecuteAsync(InsertOperation);
            var result = MemberShipsMapper.ToModel(membershipEntity);

            return result;
        }

        public async Task UpdateReport(DynamicModel report)
        {
            CloudTable table = await Connect("Reports");

            dynamic reportEntity = ReportMapper.ToEntity(report);

            var updateOperation = TableOperation.InsertOrReplace(reportEntity);

            await table.ExecuteAsync(updateOperation);
        }

        private async Task<CloudTable> Connect(string tableName)
        {
            var account = CloudStorageAccount.Parse(_settings.ConnectionString);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(tableName);
            await table.CreateIfNotExistsAsync();

            return table;
        }

        private string CreateFilter(List<Tuple<string, string>> filter)
        {
            var filterCondition = TableQuery.GenerateFilterCondition(filter[0].Item1, QueryComparisons.Equal, filter[0].Item2);

            if (filter.Count > 1)
            {
                for (int i = 1; i < filter.Count; i++)
                {

                    filterCondition = TableQuery.CombineFilters(
                                      TableQuery.GenerateFilterCondition(filter[i].Item1, QueryComparisons.Equal, filter[i].Item2),
                                      TableOperators.And,
                                      filterCondition);
                }
            }

            return filterCondition;
        }

        public async Task<DynamicModel> CheckMembership(DynamicModel membership)
        {
            CloudTable table = await Connect("Memberships");

            dynamic membershipEntity = MemberShipsMapper.ToEntity(membership);

            IEnumerable<DynamicModel> meep = await FetchMemberships(membershipEntity.project);

            foreach (dynamic meeple in meep)
            {
                if (meeple.name == membershipEntity.name)
                {
                    return null;
                }
            }

            return membership;
        }

        private TableStorageSettings _settings;
    }
}