using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VersionOne.SDK.APIClient;

namespace VersionOne.Backlog.Test
{
    [TestClass]
    public class BacklogRepositoryTest
    {
        [TestMethod]
        public void New_repository_is_dirty()
        {
            // Given a connection to a VersionOne instance defined in the app.config
            var cx = new EnvironmentContext();
            // When I create a new repository with that connection
            var repository = new BacklogRepository(cx);
            // Then it is initially dirty
            Assert.IsTrue(repository.IsDirty());
        }

        [TestMethod]
        public void Query_for_backlog_items_is_scoped_to_PrimaryWorkitem()
        {
            // Given a connection to a VersionOne instance defined in the app.config
            var cx = new EnvironmentContext();
            // And a new repository with that connection
            var repository = new BacklogRepository(cx);
            // When I build the query for my backlog items
            var query = repository.BuildQueryForMyBacklogItems();
            // Then the asset type is PrimaryWorkitems
            Assert.AreEqual("PrimaryWorkitem", query.AssetType.Token);
        }

        [TestMethod]
        public void Query_for_backlog_items_selects_number()
        {
            // Given a connection to a VersionOne instance defined in the app.config
            var cx = new EnvironmentContext();
            // And a new repository with that connection
            var repository = new BacklogRepository(cx);
            // And a reference to the PrimaryWorkitem asset type
            var assetType = cx.MetaModel.GetAssetType("PrimaryWorkitem");
            // And a reference to the number attribute
            var numberAttribute = assetType.GetAttributeDefinition("Number");
            // When I build the query for my backlog items
            var query = repository.BuildQueryForMyBacklogItems();
            // Then the query selects the name attribute
            Assert.IsTrue(query.Selection.Contains(numberAttribute));
        }

        [TestMethod]
        public void Query_for_backlog_items_selects_name()
        {
            // Given a connection to a VersionOne instance defined in the app.config
            var cx = new EnvironmentContext();
            // And a new repository with that connection
            var repository = new BacklogRepository(cx);
            // And a reference to the PrimaryWorkitem asset type
            var assetType = cx.MetaModel.GetAssetType("PrimaryWorkitem");
            // And a reference to the name attribute
            var nameAttribute = assetType.GetAttributeDefinition("Name");
            // When I build the query for my backlog items
            var query = repository.BuildQueryForMyBacklogItems();
            // Then the query selects the name attribute
            Assert.IsTrue(query.Selection.Contains(nameAttribute));
        }

        [TestMethod]
        public void Query_for_backlog_items_selects_change_date()
        {
            // Given a connection to a VersionOne instance defined in the app.config
            var cx = new EnvironmentContext();
            // And a new repository with that connection
            var repository = new BacklogRepository(cx);
            // And a reference to the PrimaryWorkitem asset type
            var assetType = cx.MetaModel.GetAssetType("PrimaryWorkitem");
            // And a reference to the change date attribute
            var changeDateAttr = assetType.GetAttributeDefinition("ChangeDateUTC");
            // When I build the query for my backlog items
            var query = repository.BuildQueryForMyBacklogItems();
            // Then the query selects the change date attribute
            Assert.IsTrue(query.Selection.Contains(changeDateAttr));
        }

        [TestMethod]
        public void Reload_is_clean()
        {
            // Given a connection to a VersionOne instance defined in the app.config
            var cx = new EnvironmentContext();
            // And a new repository with that connection
            var repository = new BacklogRepository(cx);
            // When I reload the repository
            repository.Reload();
            // Then the repository is not dirty
            Assert.IsFalse(repository.IsDirty());
        }

        [TestMethod]
        public void Retrieve_my_backlog()
        {
            // Given a connection to a VersionOne instance defined in the app.config
            var cx = new EnvironmentContext();
            // And a new repository with that connection
            var repository = new BacklogRepository(cx);
            // And that instance has a story number "B-01019"
            var storyType = cx.MetaModel.GetAssetType("Story");
            var numberAttribute = storyType.GetAttributeDefinition("Number");
            var query = new Query(storyType);
            var term = new FilterTerm(numberAttribute);
            term.Equal("B-01019");
            query.Filter = term;
            var result = cx.Services.Retrieve(query);
            if (result.TotalAvaliable == 0)
            {
                throw new Exception("The given that the instance has a story B-01019 was not met.");
            }
            // When I retrieve my backlog
            var backlog = repository.RetrieveMyBacklog();
            // Then my local cache of request categories has "B-01019"
            Assert.IsTrue(backlog.ContainsKey("B-01019"));
        }

    }
}
