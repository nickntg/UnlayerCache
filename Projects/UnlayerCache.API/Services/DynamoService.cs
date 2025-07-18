using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnlayerCache.API.Models;
using Condition = Amazon.DynamoDBv2.Model.Condition;

namespace UnlayerCache.API.Services
{
    public interface IDynamoService
    {
        Task SaveUnlayerTemplate(UnlayerCacheItem model);
        Task<string> GetUnlayerTemplate(string id);
        Task SaveUnlayerRender(UnlayerCacheItem model);
        Task<UnlayerCacheItem> GetUnlayerRender(string id);
        Task<MjmlTemplate> SaveMjmlTemplate(MjmlTemplate template);
        Task<MjmlTemplate> GetMjmlTemplate(string id);
        Task<MjmlTemplate> UpdateMjmlTemplate(MjmlTemplate template);
        Task DeleteMjmlTemplate(string id);
        Task<IList<MjmlTemplate>> ListMjmlTemplates();
    }

    public class DynamoService : IDynamoService
    {
        private const string UnlayerId             = "id";
        private const string UnlayerExpiresAt      = "expires_at";
        private const string UnlayerValue          = "value";
        private const string UnlayerTemplatesTable = "unlayer-templates";
        private const string UnlayerRendersTable   = "unlayer-renders";
        private const string MjmlTemplatesTable    = "mjml-templates";

        private readonly AppSettings _settings;
        private readonly IAmazonDynamoDB _dynamo;

        public DynamoService(AppSettings settings, IAmazonDynamoDB dynamo)
        {
            _settings = settings;
            _dynamo = dynamo;
        }

        public async Task<MjmlTemplate> SaveMjmlTemplate(MjmlTemplate template)
        {
            await DynamoHelper.Save(new UnlayerCacheItem { Id = template.Id, Value = template.Body }, _dynamo, MjmlTemplatesTable, Int32.MaxValue);

            return template;
        }

        public async Task<MjmlTemplate> GetMjmlTemplate(string id)
        {
            var template = await DynamoHelper.Get(id, _dynamo, MjmlTemplatesTable);

            return template is null
                ? null
                : new MjmlTemplate
                {
                    Id = template.Id,
                    Body = template.Value
                };
        }

        public async Task<MjmlTemplate> UpdateMjmlTemplate(MjmlTemplate template)
        {
            await DynamoHelper.Save(new UnlayerCacheItem { Id = template.Id, Value = template.Body }, _dynamo, MjmlTemplatesTable, Int32.MaxValue);

            return template;
        }

        public async Task DeleteMjmlTemplate(string id)
        {
            await DynamoHelper.Delete(id, _dynamo, MjmlTemplatesTable);
        }

        public async Task<IList<MjmlTemplate>> ListMjmlTemplates()
        {
            var results = await DynamoHelper.List(_dynamo, MjmlTemplatesTable);

            return results.Select(item => new MjmlTemplate { Id = item.Id, Body = item.Value }).ToList();
        }

        public async Task SaveUnlayerTemplate(UnlayerCacheItem model)
        {
            await DynamoHelper.Save(model, _dynamo, MjmlTemplatesTable, _settings.ExpiryInMinutes);
        }

        public async Task<string> GetUnlayerTemplate(string id)
        {
            var result = await DynamoHelper.Get(id, _dynamo, UnlayerTemplatesTable);
            return result?.Value;
        }

        public async Task SaveUnlayerRender(UnlayerCacheItem model)
        {
            await DynamoHelper.Save(model, _dynamo, UnlayerRendersTable, _settings.ExpiryInMinutes);
        }

        public async Task<UnlayerCacheItem> GetUnlayerRender(string id)
        {
            return await DynamoHelper.Get(id, _dynamo, UnlayerRendersTable);
        }

        internal class DynamoHelper
        {
            public static async Task Save(UnlayerCacheItem model, IAmazonDynamoDB dynamo, string table, int cacheExpiryInMinutes)
            {
                var lst = new Dictionary<string, AttributeValue>
                {
                    { UnlayerId, new AttributeValue { S = model.Id } },
                    { UnlayerValue, new AttributeValue { S = model.Value } },
                    {
                        UnlayerExpiresAt,
                        new AttributeValue { N = DateTimeOffset.UtcNow.AddMinutes(cacheExpiryInMinutes).ToUnixTimeSeconds().ToString() }
                    }
                };

                await dynamo.PutItemAsync(table, lst);
            }

            public static async Task Delete(string id, IAmazonDynamoDB dynamo, string table)
            {
                var request = new DeleteItemRequest
                {
                    TableName = table,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        {
                            UnlayerId, new AttributeValue { S =  id }
                        }
                    }
                };

                await dynamo.DeleteItemAsync(request);
            }

            public static async Task<IList<UnlayerCacheItem>> List(IAmazonDynamoDB dynamo, string table)
            {
                var lst = new List<UnlayerCacheItem>();

                var request = new ScanRequest
                {
                    TableName = table
                };

                do
                {
                    var response = await dynamo.ScanAsync(request);
                    foreach (var item in response.Items)
                    {
                        var r = new UnlayerCacheItem
                        {
                            Id = item[UnlayerId].S,
                            ExpiresAt = Convert.ToInt64(item[UnlayerExpiresAt].N),
                            Value = item[UnlayerValue].S
                        };

                        lst.Add(r);
                    }

                    request.ExclusiveStartKey = response.LastEvaluatedKey;
                } while (request.ExclusiveStartKey is { Count: > 0 });

                return lst;
            }

            public static async Task<UnlayerCacheItem> Get(string id, IAmazonDynamoDB dynamo, string table)
            {
                var idCondition = new Condition
                {
                    ComparisonOperator = ComparisonOperator.EQ,
                    AttributeValueList = new List<AttributeValue> { new() { S = id } }
                };

                var request = new QueryRequest
                {
                    TableName = table,
                    KeyConditions = new Dictionary<string, Condition> { { UnlayerId, idCondition } },
                    Select = Select.ALL_ATTRIBUTES,
                    ConsistentRead = true
                };

                var result = await dynamo.QueryAsync(request);

                if (result.Count == 0)
                {
                    return null;
                }

                var item = new UnlayerCacheItem
                {
                    Id = result.Items[0][UnlayerId].S,
                    ExpiresAt = Convert.ToInt64(result.Items[0][UnlayerExpiresAt].N),
                    Value = result.Items[0][UnlayerValue].S
                };

                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > item.ExpiresAt)
                {
                    /*
                     * Unfortunately, there are too many conditions where
                     * dynamo may have not automatically cleared the item
                     * based on the TTL. So we do the dirty work.
                     */
                    await Delete(item.Id, dynamo, table);

                    return null;
                }

                return item;
            }
        }
    }
}
