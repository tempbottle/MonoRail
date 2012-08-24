﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Castle.MonoRail.Extension.OData;
using Castle.MonoRail.Extension.OData3.Tests;
using Castle.MonoRail.Tests;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using NUnit.Framework;
using FluentAssertions;

namespace Castle.MonoRail.Extension.OData.Tests
{
	[TestFixture]
	public class SegmentParserTestCase
	{
		private UriSegment[] Segments;
		private MetaSegment Meta;
		private MetaQuerySegment[] MetaQueries;
		private IEdmModel _emptyModel = new EdmModel();

		protected UriSegment[] Parse(string path, string qs, IEdmModel model)
		{
			var parameters = Utils.BuildFromQS(qs);

			var tuple = SegmentParser.parse(path, parameters, model, new Uri("http://localhost/base/"));

			Segments = tuple.Item1;
			Meta = tuple.Item2;
			MetaQueries = tuple.Item3;

			return Segments;
		}

		[Test]
		public void ServiceDirectory_()
		{
			var segments = Parse("/", string.Empty, _emptyModel);
			Asserts.FirstSegmentIsServiceDirectory(segments);
		}

		[Test]
		public void ServiceDirectory_2()
		{
			var segments = Parse(string.Empty, string.Empty, _emptyModel);
			Asserts.FirstSegmentIsServiceDirectory(segments);
		}

		[Test]
		public void MetadataIdentifier_()
		{
			var segments = Parse("/$metadata", String.Empty, _emptyModel);
			Asserts.FirstSegmentIsNothing(segments);
			Asserts.IsMeta_Metadata(this.Meta);
		}

		[Test, ExpectedException(typeof (HttpException), ExpectedMessage = "First segment of uri could not be parsed")]
		public void AccessingEntity_ThatDoesNotExists_HttpError()
		{
			Parse("/catalogs", String.Empty, _emptyModel);
		}

		[Test]
		public void AccessingEntity_()
		{
			var odataModel = new Models.SimpleODataModel();
			odataModel.InitializeModels(new StubServiceRegistry());
			var model = odataModel.EdmModel;
			var productsEntSet = model.EntityContainers().Single().FindEntitySet("Products");

			var segments = Parse("/products", String.Empty, model);
			Asserts.ExpectingSegmentsCount(segments, 1);
			Asserts.IsEntitySet(segments.ElementAt(0), "products", productsEntSet.ElementType);
		}

		[Test]
		public void AccessingEntityByKey_()
		{
            var odataModel = new Models.SimpleODataModel();
            odataModel.InitializeModels(new StubServiceRegistry());
            var model = odataModel.EdmModel;
            var segments = Parse("/products(1)", String.Empty, model);
//			Asserts.ExpectingSegmentsCount(segments, 1);
//			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs",
//			                     resource: model.GetResourceSet("catalogs").Value.ResourceType);
		}

		[Test]
		public void AccessingEntityByKey_2()
		{
            var odataModel = new Models.SimpleODataModel();
            odataModel.InitializeModels(new StubServiceRegistry());
            var model = odataModel.EdmModel;

            var segments = Parse("/products(10)", String.Empty, model);

//            Asserts.ExpectingSegmentsCount(segments, 1);
//			Asserts.IsEntityType(segments.ElementAt(0), Key: "10", Name: "catalogs",
//			                     resource: model.GetResourceSet("catalogs").Value.ResourceType);
		}

		[Test]
		public void AccessingEntityAndProp_()
		{
            var odataModel = new Models.SimpleODataModel();
            odataModel.InitializeModels(new StubServiceRegistry());
            var model = odataModel.EdmModel;
            
            var segments = Parse("/products(1)/Id", String.Empty, model);

			Asserts.ExpectingSegmentsCount(segments, 2);
//			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs",
//			                     resource: model.GetResourceSet("catalogs").Value.ResourceType);
//			Asserts.IsPropertySingle(segments.ElementAt(1), name: "Id", relativeUri: "/catalogs(1)/Id");
		}

		[Test]
		public void AccessingEntityAndProp_2()
		{
            var odataModel = new Models.SimpleODataModel();
            odataModel.InitializeModels(new StubServiceRegistry());
            var model = odataModel.EdmModel;

            var segments = Parse("/products(1)/Name", String.Empty, model);

			Asserts.ExpectingSegmentsCount(segments, 2);
//			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs",
//			                     resource: model.GetResourceSet("catalogs").Value.ResourceType);
//			Asserts.IsPropertySingle(segments.ElementAt(1), name: "Name", relativeUri: "/catalogs(1)/Name");
		}

		[Test, ExpectedException(typeof (HttpException), ExpectedMessage = "Segment does not match a property or operation")]
		public void AccessingEntity_NonExistingProperty()
		{
            var odataModel = new Models.SimpleODataModel();
            odataModel.InitializeModels(new StubServiceRegistry());
            var model = odataModel.EdmModel;

			Parse("/products(1)/Something", String.Empty, model);
		}

		[Test]
		public void AccessingEntityAndPropValue_()
		{
            var odataModel = new Models.SimpleODataModel();
            odataModel.InitializeModels(new StubServiceRegistry());
            var model = odataModel.EdmModel;

            var segments = Parse("/products(1)/Id/$value", String.Empty, model);
			Asserts.ExpectingSegmentsCount(segments, 2);
//			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs",
//			                     resource: model.GetResourceSet("catalogs").Value.ResourceType);
//			Asserts.IsPropertySingle(segments.ElementAt(1), "Id", relativeUri: "/catalogs(1)/Id");
			Asserts.IsMeta_Value(this.Meta);
		}

		[Test]
		public void AccessingEntityAndPropValue_2()
		{
            var odataModel = new Models.SimpleODataModel();
            odataModel.InitializeModels(new StubServiceRegistry());
            var model = odataModel.EdmModel;

			var segments = Parse("/products(1)/Name/$value", String.Empty, model);
			Asserts.ExpectingSegmentsCount(segments, 2);
//			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs",
//			                     resource: model.GetResourceSet("catalogs").Value.ResourceType);
//			Asserts.IsPropertySingle(segments.ElementAt(1), "Name", relativeUri: "/catalogs(1)/Name");
			Asserts.IsMeta_Value(this.Meta);
		}

//		[Test]
//		public void AccessingEntity_And_OneToManyRelationship_1()
//		{
//			var model = new StubModel(
//				m =>
//					{
//						m.EntitySet("catalogs", new List<Catalog2>().AsQueryable());
//						m.EntitySet("products", new List<Product2>().AsQueryable());
//					});
//			var services = new StubServiceRegistry();
//			model.Initialize(services);
//			var segments = Parse("/catalogs(1)/Products/", String.Empty, model);
//			Asserts.ExpectingSegmentsCount(segments, 2);
//			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs",
//			                     resource: model.GetResourceSet("catalogs").Value.ResourceType);
//
//			// Not sure which one this should be? Products or catalog? 
//			Asserts.IsPropertyCollection(segments.ElementAt(1), Name: "Products",
//			                             resource: model.GetResourceType("Product2").Value, relativeUri: "/catalogs(1)/Products");
//		}

//		[Test]
//		public void AccessingEntity_And_OneToManyRelationshipWithKey_()
//		{
//			var model = new StubModel(
//				m =>
//					{
//						m.EntitySet("catalogs", new List<Catalog2>().AsQueryable());
//						m.EntitySet("products", new List<Product2>().AsQueryable());
//					});
//			var services = new StubServiceRegistry();
//			model.Initialize(services);
//			var segments = Parse("/catalogs(1)/Products(2)/", String.Empty, model);
//			Asserts.ExpectingSegmentsCount(segments, 2);
//			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs",
//			                     resource: model.GetResourceSet("catalogs").Value.ResourceType);
//			Asserts.IsPropertySingle(segments.ElementAt(1), name: "Products", key: "2", relativeUri: "/catalogs(1)/Products(2)");
//		}
//
//		[Test]
//		public void AccessingEntity_And_OneToManyRelationshipWithKey_And_Property()
//		{
//			var model = new StubModel(
//				m =>
//					{
//						m.EntitySet("catalogs", new List<Catalog2>().AsQueryable());
//						m.EntitySet("products", new List<Product2>().AsQueryable());
//					});
//			var services = new StubServiceRegistry();
//			model.Initialize(services);
//			var segments = Parse("/catalogs(1)/Products(2)/Name", String.Empty, model);
//			Asserts.ExpectingSegmentsCount(segments, 3);
//			Asserts.IsEntityType(segments.ElementAt(0), Key: "1", Name: "catalogs",
//			                     resource: model.GetResourceSet("catalogs").Value.ResourceType);
//			Asserts.IsPropertySingle(segments.ElementAt(1), name: "Products", key: "2");
//			Asserts.IsPropertySingle(segments.ElementAt(2), name: "Name");
//		}

//		public class Catalog1
//		{
//			[Key]
//			public int Id { get; set; }
//
//			public string Name { get; set; }
//		}
//
//		public class Product2
//		{
//			[Key]
//			public int Id { get; set; }
//
//			public string Name { get; set; }
//		}
//
//		public class Catalog2
//		{
//			[Key]
//			public int Id { get; set; }
//
//			public string Name { get; set; }
//			public IList<Product2> Products { get; set; }
//		}
//
		public static class Asserts
		{
			public static void FirstSegmentIsServiceDirectory(UriSegment[] segments)
			{
				ExpectingSegmentsCount(segments, 1);
				segments.ElementAt(0).IsServiceDirectory.Should().BeTrue();
			}

			public static void FirstSegmentIsNothing(UriSegment[] segments)
			{
				ExpectingSegmentsCount(segments, 1);
				segments.ElementAt(0).IsNothing.Should().BeTrue();
				//segments.ElementAt(0).As<UriSegment.Meta>().item.IsMetadata.Should().BeTrue();
			}

			public static void ExpectingSegmentsCount(UriSegment[] segments, int count)
			{
				segments.Should().NotBeNull();
				segments.Should().HaveCount(count);
			}

			public static void IsEntitySet(UriSegment seg, string Name, IEdmEntityType resource, string relativeUri = null)
			{
				var segment = seg.As<UriSegment.EntitySet>();
				segment.Should().NotBeNull();
				segment.item.Key.Should().BeNull();
				segment.item.Name.Should().Be(Name);
				segment.item.EdmEntityType.Should().Be(resource);
				segment.item.RawPathSegment.Should().Be(Name);
				// if (relativeUri != null) 
				segment.item.Uri.OriginalString.Should().BeEquivalentTo("http://localhost/base/" + Name);
			}

//			public static void IsEntityType(UriSegment seg, string Key, string Name, ResourceType resource,
//			                                string relativeUri = null)
//			{
//				var segment = seg.As<UriSegment.EntityType>();
//				segment.Should().NotBeNull();
//				segment.item.Key.Should().Be(Key);
//				segment.item.Name.Should().Be(Name);
//				segment.item.ResourceType.Should().Be(resource);
//				segment.item.RawPathSegment.Should().Be(Name + "(" + Key + ")");
//				// if (relativeUri != null) 
//				segment.item.Uri.OriginalString.Should().BeEquivalentTo("http://localhost/base/" + Name + "(" + Key + ")");
//			}
//
//			public static void IsPropertySingle(UriSegment elementAt, string name, string key = null, string relativeUri = null)
//			{
//				var segment = elementAt.As<UriSegment.PropertyAccessSingle>();
//				segment.Should().NotBeNull();
//				segment.item.Property.Name.Should().Be(name);
//				segment.item.ResourceType.Should().NotBeNull();
//				if (relativeUri != null)
//				{
//					segment.item.Uri.OriginalString.Should().BeEquivalentTo("http://localhost/base" + relativeUri);
//				}
//				if (key == null)
//				{
//					segment.item.Key.Should().BeNull();
//					segment.item.RawPathSegment.Should().Be(name);
//				}
//				else
//				{
//					segment.item.RawPathSegment.Should().Be(name + "(" + key + ")");
//					segment.item.Key.Should().NotBeNull();
//					segment.item.Key.Should().Be(key);
//				}
//			}
//
//			public static void IsPropertyCollection(UriSegment elementAt, string Name, ResourceType resource,
//			                                        string relativeUri = null)
//			{
//				var segment = elementAt.As<UriSegment.PropertyAccessCollection>();
//				segment.Should().NotBeNull();
//				segment.item.Property.Name.Should().Be(Name);
//				segment.item.ResourceType.Should().NotBeNull();
//				segment.item.ResourceType.Should().Be(resource);
//				segment.item.Key.Should().BeNull();
//				segment.item.RawPathSegment.Should().BeEquivalentTo(Name);
//				if (relativeUri != null)
//					segment.item.Uri.OriginalString.Should().BeEquivalentTo("http://localhost/base" + relativeUri);
//			}
//
			public static void IsMeta_Metadata(MetaSegment meta)
			{
				meta.IsMetadata.Should().BeTrue("Expected MetaSegment to be $metadata");
			}

			public static void IsMeta_Value(MetaSegment meta)
			{
				meta.IsValue.Should().BeTrue("Expected MetaSegment to be $value");
			}

			public static void IsMeta_Batch(MetaSegment meta)
			{
				meta.IsBatch.Should().BeTrue("Expected MetaSegment to be $batch");
			}

			public static void IsMeta_Count(MetaSegment meta)
			{
				meta.IsCount.Should().BeTrue("Expected MetaSegment to be $meta");
			}

//			public static void IsMeta_Links(MetaSegment meta)
//			{
//				meta.IsLinks.Should().BeTrue("Expected MetaSegment to be $links");
//			}
		}

	}
}