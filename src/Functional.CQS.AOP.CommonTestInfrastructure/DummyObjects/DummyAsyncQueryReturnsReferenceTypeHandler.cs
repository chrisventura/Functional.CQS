﻿using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Functional.CQS.AOP.CommonTestInfrastructure.DummyObjects
{
	// ReSharper disable once InconsistentNaming
	/// <summary>
	/// Sample <see cref="IAsyncQueryHandler{TQuery, TResult}"/> implementation.  Returns reference type.
	/// </summary>
	public class DummyAsyncQueryReturnsReferenceTypeHandler : IAsyncQueryHandler<DummyAsyncQueryReturnsReferenceType, DummyAsyncQueryReturnsReferenceTypeResult>
	{
		private static readonly DummyAsyncQueryReturnsReferenceType _query = new DummyAsyncQueryReturnsReferenceType();
		private static readonly DummyAsyncQueryReturnsReferenceTypeResult _result = new DummyAsyncQueryReturnsReferenceTypeResult();

		/// <summary>
		/// Handle the query asynchronously.
		/// </summary>
		/// <param name="query">The query parameters.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public async Task<DummyAsyncQueryReturnsReferenceTypeResult> HandleAsync(DummyAsyncQueryReturnsReferenceType query, CancellationToken cancellationToken = new CancellationToken())
		{
			return await Task.Run(() => _result, cancellationToken);
		}
	}
}