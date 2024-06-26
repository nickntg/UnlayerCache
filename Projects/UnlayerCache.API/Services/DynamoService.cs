﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using UnlayerCache.API.Models;

namespace UnlayerCache.API.Services
{
    public interface IDynamoService
    {
        Task SaveUnlayerTemplate(UnlayerCacheItem model);
        Task<string> GetUnlayerTemplate(string id);
        Task SaveUnlayerRender(UnlayerCacheItem model);
        Task<UnlayerCacheItem> GetUnlayerRender(string id);
    }

    public class DynamoService : IDynamoService
    {
        private const string UnlayerId             = "id";
        private const string UnlayerExpiresAt      = "expires_at";
        private const string UnlayerValue          = "value";
        private const string UnlayerTemplatesTable = "unlayer-templates";
        private const string UnlayerRendersTable   = "unlayer-renders";

        private readonly AppSettings _settings;
        private readonly IAmazonDynamoDB _dynamo;

        public DynamoService(AppSettings settings, IAmazonDynamoDB dynamo)
        {
            _settings = settings;
            _dynamo = dynamo;
        }

        public async Task SaveUnlayerTemplate(UnlayerCacheItem model)
        {
            await DynamoHelper<UnlayerCacheItem>.Save(model, _dynamo, UnlayerTemplatesTable, _settings.ExpiryInMinutes);
        }

        public async Task<string> GetUnlayerTemplate(string id)
        {
            var result = await DynamoHelper<UnlayerCacheItem>.Get(id, _dynamo, UnlayerTemplatesTable);
            return result?.Value;
        }

        public async Task SaveUnlayerRender(UnlayerCacheItem model)
        {
            await DynamoHelper<UnlayerCacheItem>.Save(model, _dynamo, UnlayerRendersTable, _settings.ExpiryInMinutes);
        }

        public async Task<UnlayerCacheItem> GetUnlayerRender(string id)
        {
            return await DynamoHelper<UnlayerCacheItem>.Get(id, _dynamo, UnlayerRendersTable);
        }

        internal class DynamoHelper<T> where T: UnlayerCacheItem
        {
            public static async Task Save(T model, IAmazonDynamoDB dynamo, string table, int cacheExpiryInMinutes)
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

            public static async Task Delete(T model, IAmazonDynamoDB dynamo, string table)
            {
                var request = new DeleteItemRequest
                {
                    TableName = table,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        {
                            UnlayerId, new AttributeValue { S =  model.Id }
                        }
                    }
                };

                await dynamo.DeleteItemAsync(request);
            }

            public static async Task<T> Get(string id, IAmazonDynamoDB dynamo, string table)
            {
                var idCondition = new Condition
                {
                    ComparisonOperator = ComparisonOperator.EQ,
                    AttributeValueList = new List<AttributeValue> { new AttributeValue { S = id } }
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
                    await Delete((T)item, dynamo, table);

                    return null;
                }

                return (T)item;
            }
        }
    }
}
