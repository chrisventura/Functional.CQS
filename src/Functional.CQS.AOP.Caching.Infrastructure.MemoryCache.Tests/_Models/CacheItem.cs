﻿using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Functional.CQS.AOP.Caching.Infrastructure.MemoryCache.Tests
{
	internal class CacheItem
	{
		public CacheItem(string key, Option<string> groupKey)
		{
			Key = key;
			GroupKey = groupKey;
		}

		public string Key { get; }
		public Option<string> GroupKey { get; }
	}
}
