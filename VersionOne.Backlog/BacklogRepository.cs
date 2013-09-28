using System;
using System.Collections.Generic;
using System.Globalization;
using VersionOne.SDK.APIClient;

namespace VersionOne.Backlog
{
    public class BacklogRepository
    {
        private const string BacklogName = "CallCenter";
        private readonly EnvironmentContext _cx;
        private readonly IAssetType _backlogItemType;
        private readonly IAttributeDefinition _numberAttribute;
        private readonly IAttributeDefinition _nameAttribute;
        private readonly IAttributeDefinition _customAttribute;
        private readonly IAttributeDefinition _changeAttribute;
        private readonly IAttributeDefinition _parentScopesAttribute;
        private readonly Query _queryForMyBacklogItems;
        private DateTime? _mostRecentChangeDateTime;
        private IDictionary<string, BacklogItem> _myBacklogItems;

        public BacklogRepository(EnvironmentContext context)
        {
            _cx = context;
            _backlogItemType = _cx.MetaModel.GetAssetType("PrimaryWorkitem");
            _numberAttribute = _backlogItemType.GetAttributeDefinition("Number");
            _nameAttribute = _backlogItemType.GetAttributeDefinition("Name");
            _customAttribute = _backlogItemType.GetAttributeDefinition("Custom_Tags2");
            _changeAttribute = _backlogItemType.GetAttributeDefinition("ChangeDateUTC");
            _parentScopesAttribute = _backlogItemType.GetAttributeDefinition("ParentMeAndUp.Name");
            _queryForMyBacklogItems = BuildQueryForMyBacklogItems();
        }

        public Query BuildQueryForMyBacklogItems()
        {
            var query = new Query(_backlogItemType);
            query.Selection.Add(_numberAttribute);
            query.Selection.Add(_nameAttribute);
            query.Selection.Add(_changeAttribute);
            var projectFilter = new FilterTerm(_parentScopesAttribute);
            projectFilter.Equal(BacklogName);
            return query;
        }

        public bool IsDirty()
        {
            if (!_mostRecentChangeDateTime.HasValue)
            {
                return true;
            }
            var query = new Query(_backlogItemType);
            query.Selection.Add(_changeAttribute);
            var term = new FilterTerm(_changeAttribute);
            term.Greater(_mostRecentChangeDateTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture));
            query.Filter = term;
            var result = _cx.Services.Retrieve(query);
            return result.TotalAvaliable > 0;
        }

        public void Reload()
        {
            _myBacklogItems = new Dictionary<string, BacklogItem>();
            var result = _cx.Services.Retrieve(_queryForMyBacklogItems);
            foreach (var asset in result.Assets)
            {
                var backlogItem = new BacklogItem
                    {
                        // Name is required so it won't be null
                        Name = asset.GetAttribute(_nameAttribute).Value.ToString(),
                        // Custom is not required, so it might be null
                        Custom = (null==asset.GetAttribute(_customAttribute)) ? null : asset.GetAttribute(_customAttribute).Value.ToString()
                    };
                _myBacklogItems.Add(asset.GetAttribute(_numberAttribute).Value.ToString(), backlogItem);
                // Remember the most recent change to VersionOne for checking dirty state
                var changeDateTime = DB.DateTime(asset.GetAttribute(_changeAttribute).Value);
                if ((!_mostRecentChangeDateTime.HasValue) || (changeDateTime > _mostRecentChangeDateTime))
                {
                    _mostRecentChangeDateTime = changeDateTime;
                }
            }
        }

        /// <summary>
        /// Poll server for new or updated backlog items. If there is something new, then retreives the whole list.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, BacklogItem> RetrieveMyBacklog()
        {
            if (IsDirty())
            {
                Reload();
            }
            return _myBacklogItems;
        }

    }
}
