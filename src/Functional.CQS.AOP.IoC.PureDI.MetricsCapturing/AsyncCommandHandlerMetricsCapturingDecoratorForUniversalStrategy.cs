﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Functional.CQS.AOP.IoC.PureDI.MetricsCapturing.Extensions;
using Functional.CQS.AOP.MetricsCapturing;

namespace Functional.CQS.AOP.IoC.PureDI.MetricsCapturing
{
	/// <summary>
	/// Decorator for applying universal metrics-capturing concerns to <see cref="IAsyncCommandHandler{TCommand,TError}"/>.
	/// </summary>
	/// <typeparam name="TCommand">The command type.</typeparam>
	/// <typeparam name="TError">The error type.</typeparam>
	public class AsyncCommandHandlerMetricsCapturingDecoratorForUniversalStrategy<TCommand, TError> : IAsyncCommandHandler<TCommand, TError>
		where TCommand : ICommandParameters<TError>
	{
		private readonly IAsyncCommandHandler<TCommand, TError> _handler;
		private readonly IUniversalMetricsCapturingStrategy _strategy;

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCommandHandlerMetricsCapturingDecoratorForUniversalStrategy{TCommand, TError}"/> class.
		/// </summary>
		/// <param name="handler">The handler to decorate.</param>
		/// <param name="strategy">The metrics-capturing strategy.</param>
		public AsyncCommandHandlerMetricsCapturingDecoratorForUniversalStrategy(
			IAsyncCommandHandler<TCommand, TError> handler,
			IUniversalMetricsCapturingStrategy strategy)
		{
			_handler = handler ?? throw new ArgumentNullException(nameof(handler));
			_strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
		}

		/// <summary>
		/// Handle the command.
		/// </summary>
		/// <param name="command">The command parameters.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public async Task<Result<Unit, TError>> HandleAsync(TCommand command, CancellationToken cancellationToken)
		{
			return await _handler.HandleAsyncWithMetricsCapturing(command, cancellationToken,
				c => _strategy.OnInvocationStart(),
				(c, result, timeElapsed) => _strategy.OnInvocationCompletedSuccessfully(timeElapsed),
				(c, exception, timeElapsed) => _strategy.OnInvocationException(exception, timeElapsed));
		}
	}
}