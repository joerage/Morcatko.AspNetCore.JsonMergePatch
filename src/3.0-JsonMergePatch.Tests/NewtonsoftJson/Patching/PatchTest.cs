﻿using Morcatko.AspNetCore.JsonMergePatch.NewtonsoftJson.Builders;
using System.Linq;
using Xunit;

namespace Morcatko.AspNetCore.JsonMergePatch.Tests.NewtonsoftJson.Patching
{
	public class PatchTests
	{
		private NewtonsoftTestModel GetTestModel() =>
		   new NewtonsoftTestModel()
		   {
			   Integer = 5,
			   String = "string",
			   Float = 1.5f,
			   Boolean = false,
			   Renamed = "some string",
			   SimpleEnum = SimpleEnum.two,
			   ValueEnum = ValueEnum.i,
			   SubModel = new SubModel()
			   {
				   Value1 = "value 1",
				   Value2 = "value 2",
				   Numbers = new[] { 1, 2, 3 }
			   }
		   };

		private JsonMergePatchDocument<NewtonsoftTestModel> GetTestPatch(object patchObject, JsonMergePatchOptions options = null)
			=> PatchBuilder.Build<NewtonsoftTestModel>(patchObject, options);

		private NewtonsoftTestModel GetPatchedModel(object patchObject, NewtonsoftTestModel model = null, JsonMergePatchOptions options = null)
			=> GetTestPatch(patchObject, options).ApplyTo(model ?? GetTestModel());

		[Fact]
		public void PatchInteger()
		{
			var patchedModel = GetPatchedModel(
				new { integer = 8 });

			var expected = GetTestModel();
			expected.Integer = 8;
			Assert.Equal(expected, patchedModel);
		}

		[Fact]
		public void PatchSimpleEnum()
		{
			var patchedModel = GetPatchedModel(
				new { simpleEnum = "one" });

			var expected = GetTestModel();
			expected.SimpleEnum = SimpleEnum.one;
			Assert.Equal(expected, patchedModel);
		}

		[Fact]
		public void PatchSubProperty()
		{
			var patchedModel = GetPatchedModel(
				new { subModel = new { value1 = "new value 1" } });

			var expected = GetTestModel();
			expected.SubModel.Value1 = "new value 1";
			Assert.Equal(expected, patchedModel);
		}

		[Fact]
		public void PatchDictionnaryAddKey()
		{
			var patchedModel = GetPatchedModel(
				new { subModels = new { test = new { } } });

			var expected = GetTestModel();
			expected.SubModels["test"] = new SubModel();
			Assert.Equal(expected, patchedModel);
		}

		[Fact]
		public void PatchDictionnaryAddKeyWithSlash()
		{
			var patchString = @"{ ""SubModels"": {""te/st"": {} } }";
			JsonMergePatchDocument<TestModelBase> patch = PatchBuilder.Build<TestModelBase>(patchString);
			TestModelBase patchedModel = patch.ApplyTo(GetTestModel());
			Assert.Equal("te/st", patchedModel.SubModels.First().Key);
		}

		[Fact]
		public void PatchDictionnaryEditProperty()
		{
			var model = GetTestModel();
			model.SubModels["test"] = new SubModel { Value1 = "a value" };

			var patchedModel = GetPatchedModel(
				new { subModels = new { test = new { value1 = "a new value" } } },
				model);

			var expected = GetTestModel();
			expected.SubModels["test"] = new SubModel { Value1 = "a new value" };
			Assert.Equal(expected, patchedModel);
		}

		[Fact]
		public void PatchDoesNotRemoveADictionaryEntry()
		{
			var model = GetTestModel();
			model.SubModels["test"] = new SubModel { Value1 = "a value" };

			var patchedModel = GetPatchedModel(
				new { subModels = new { test = (object)null } },
				model);

			var expected = GetTestModel();
			expected.SubModels["test"] = null;
			Assert.Equal(expected, patchedModel);
		}

		[Fact]
		public void PatchRemoveADictionaryEntryWhenOption()
		{
			var model = GetTestModel();
			model.SubModels["test"] = new SubModel { Value1 = "a value" };

			var patchedModel = GetPatchedModel(
				new { subModels = new { test = (object)null } },
				model,
				new JsonMergePatchOptions() { EnableDelete = true });

			var expected = GetTestModel();
			Assert.Equal(expected, patchedModel);
		}

		[Fact]
		public void PatchSubPropertyOfNotExistingObject()
		{
			var model = GetTestModel();
			model.SubModel = null;

			var patchedModel = GetPatchedModel(
				new { subModel = new { value1 = "new value 1" } },
				model);

			var expected = GetTestModel();
			expected.SubModel = new SubModel() { Value1 = "new value 1" };
			Assert.Equal(expected, patchedModel);
		}


		[Fact]
		public void PatchTwoSubPropertiesOfNotExistingObject()
		{
			var model = GetTestModel();
			model.SubModel = null;

			var patchedModel = GetPatchedModel(
				new { subModel = new { value1 = "new value 1", numbers = new[] { 1, 3 } } },
				model);

			var expected = GetTestModel();
			expected.SubModel = new SubModel() { Value1 = "new value 1", Numbers = new[] { 1, 3 } };
			Assert.Equal(expected, patchedModel);
		}


		[Fact]
		public void PatchAddAnObjectToANullProperty()
		{
			var model = GetTestModel();
			model.SubModel = null;

			var patchedModel = GetPatchedModel(
				new { subModel = new { } },
				model);

			var expected = GetTestModel();
			expected.SubModel = new SubModel();
			Assert.Equal(expected, patchedModel);
		}


		[Fact]
		public void PatchSubSubObjectEmpty()
		{
			var model = GetTestModel();
			model.SubModel = null;

			var patchedModel = GetPatchedModel(
				new { subModel = new { subSubModel = new { } } },
				model);

			var expected = GetTestModel();
			expected.SubModel = new SubModel { SubSubModel = new SubSubModel() };
			Assert.Equal(expected, patchedModel);
		}


		[Fact]
		public void PatchRemoveSubSubObjectEmpty()
		{
			var model = GetTestModel();

			var patchedModel = GetPatchedModel(
				new { subModel = new { subSubModel = (object)null } },
				model);

			var expected = GetTestModel();
			expected.SubModel.SubSubModel = null;
			Assert.Equal(expected, patchedModel);
		}


		[Fact]
		public void PatchSubSubObjectProperty()
		{
			var model = GetTestModel();
			model.SubModel = null;

			var patchedModel = GetPatchedModel(
				new { subModel = new { subSubModel = new { value1 = "new value 1" } } },
				model);

			var expected = GetTestModel();
			expected.SubModel = new SubModel { SubSubModel = new SubSubModel { Value1 = "new value 1" } };
			Assert.Equal(expected, patchedModel);
		}


		[Fact]
		public void PatchObject()
		{
			var patchedModel = GetPatchedModel(
				new { subModel = new SubModel() { Value1 = "All others are null" } });

			var expected = GetTestModel();
			expected.SubModel = new SubModel() { Value1 = "All others are null" };
			Assert.Equal(expected, patchedModel);
		}


		[Fact]
		public void NullObject()
		{
			var patchedModel = GetPatchedModel(
				new { subModel = null as SubModel });

			var expected = GetTestModel();
			expected.SubModel = null;
			Assert.Equal(expected, patchedModel);
		}

	}
}
