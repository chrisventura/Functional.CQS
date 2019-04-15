﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using Functional.CQS.AOP.Caching.Infrastructure.DistributedCache.Redis.JsonConverters;
using Functional.CQS.AOP.Caching.Infrastructure.DistributedCache.Redis.Tests.JsonConverters.Models;
using Functional.Primitives.FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Functional.CQS.AOP.Caching.Infrastructure.DistributedCache.Redis.Tests.JsonConverters
{
	public class ResultJsonConverterTests
	{
		static ResultJsonConverterTests()
		{
			JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
			{
				Converters = new List<JsonConverter> { new ResultJsonConverter() }
			};
		}

		[Fact]
		public void ShouldBeAbleToConvertSuccessfulResult()
		{
			new ResultJsonConverter().CanConvert(Result.Success<int, string>(1337).GetType()).Should().BeTrue();
			new ResultJsonConverter().CanConvert(Result.Success<AppModel, Exception>(AppModel.Create()).GetType()).Should().BeTrue();
			new ResultJsonConverter().CanConvert(Result.Success<AppModelWithVersion, Exception>(AppModelWithVersion.Create()).GetType()).Should().BeTrue();
		}

		[Fact]
		public void ShouldBeAbleToSerializeAndDeserializeSuccessfulResult()
		{
			const int SUCCESS_VALUE = 1337;
			var json = JsonConvert.SerializeObject(Result.Success<int, string>(SUCCESS_VALUE));
			var fromJson = (Result<int, string>)JsonConvert.DeserializeObject(json, typeof(Result<int, string>));

			fromJson.Should().BeSuccessfulWithExpectedValue(SUCCESS_VALUE);
		}

		[Fact]
		public void ShouldBeAbleToSerializeAndDeserializeSuccessfulResultOfSimplePOCO()
		{
			var obj = AppModel.Create();
			var json = JsonConvert.SerializeObject(Result.Success<AppModel, Exception>(obj));
			var fromJson = (Result<AppModel, Exception>)JsonConvert.DeserializeObject(json, typeof(Result<AppModel, Exception>));

			fromJson.Should().BeSuccessful(x => x.IsLike(obj));
		}

		[Fact]
		public void ShouldBeAbleToSerializeAndDeserializeSuccessfulResultOfComplexPOCO()
		{
			var obj = AppModelWithVersion.Create();
			var json = JsonConvert.SerializeObject(Result.Success<AppModelWithVersion, Exception>(obj));
			var fromJson = (Result<AppModelWithVersion, Exception>)JsonConvert.DeserializeObject(json, typeof(Result<AppModelWithVersion, Exception>));

			fromJson.Should().BeSuccessful(x => x.IsLike(obj));
		}

		[Fact]
		public void ShouldBeAbleToConvertFaultedResult()
		{
			new ResultJsonConverter().CanConvert(Result.Failure<int, string>("dead or alive, you're coming with me").GetType()).Should().BeTrue();
			new ResultJsonConverter().CanConvert(Result.Failure<AppModel, Exception>(new Exception("some error")).GetType()).Should().BeTrue();
			new ResultJsonConverter().CanConvert(Result.Failure<AppModelWithVersion, Exception>(new Exception("ERROR!", new Exception("inner error"))).GetType()).Should().BeTrue();
		}

		[Fact]
		public void ShouldBeAbleToSerializeAndDeserializeFaultedResult()
		{
			const string FAIL_VALUE = "dead or alive, you're coming with me";
			var json = JsonConvert.SerializeObject(Result.Failure<int, string>(FAIL_VALUE));
			var fromJson = JsonConvert.DeserializeObject<Result<int, string>>(json, new ResultJsonConverter());

			fromJson.Should().BeFaultedWithExpectedValue(FAIL_VALUE);
		}
	}
}
