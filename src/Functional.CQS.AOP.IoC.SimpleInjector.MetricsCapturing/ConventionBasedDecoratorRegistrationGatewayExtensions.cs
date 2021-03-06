﻿using System;
using System.Collections.Generic;
using System.Linq;
using Functional;
using Functional.CQS;
using Functional.CQS.AOP.IoC.PureDI.MetricsCapturing;
using Functional.CQS.AOP.IoC.SimpleInjector.DecoratorRegistrationGateways;
using Functional.CQS.AOP.IoC.SimpleInjector.MetricsCapturing;
using Functional.CQS.AOP.IoC.SimpleInjector.MetricsCapturing.Configuration;
using Functional.CQS.AOP.IoC.SimpleInjector.Models;
using Functional.CQS.AOP.MetricsCapturing;

// ReSharper disable once CheckNamespace
namespace SimpleInjector
{
	/// <summary>
	/// Extension methods for <see cref="ConventionBasedDecoratorRegistrationGateway"/>.
	/// </summary>
	public static class ConventionBasedDecoratorRegistrationGatewayExtensions
	{
		/// <summary>
		/// Register all core components required for applying metrics-capturing decorators to Functional.CQS handler implementations.
		/// Only handlers with corresponding <see cref="IMetricsCapturingStrategyForQuery{TQuery, TResult}"/> and <see cref="IMetricsCapturingStrategyForCommand{TCommand, TError}"/> implementations will have the handler-specific metrics-capturing decorator applied to them.
		/// No universal metrics-capturing decorator will be applied.
		/// </summary>
		/// <param name="gateway">The gateway.</param>
		/// <param name="configurationParameters">The configuration parameters.</param>
		/// <returns></returns>
		public static ConventionBasedDecoratorRegistrationGateway WithMetricsCapturingDecorator(this ConventionBasedDecoratorRegistrationGateway gateway, MetricsCapturingModuleConfigurationParameters configurationParameters)
		{
			gateway.RegisterMetricsCapturingDecoratorForIndividualQueryHandlerImplementations(configurationParameters);
			gateway.RegisterMetricsCapturingDecoratorForIndividualCommandHandlerImplementations(configurationParameters);

			return gateway;
		}

		/// <summary>
		/// Register all core components required for applying metrics-capturing decorators to Functional.CQS handler implementations.
		/// Only handlers with corresponding <see cref="IMetricsCapturingStrategyForQuery{TQuery, TResult}"/> and <see cref="IMetricsCapturingStrategyForCommand{TCommand, TError}"/> implementations will have the handler-specific metrics-capturing decorator applied to them.
		/// All handler implementations will have the universal metrics-capturing decorator applied to them.
		/// </summary>
		/// <typeparam name="TUniversalMetricsCapturingStrategy">The universal metrics capturing strategy type.</typeparam>
		/// <param name="gateway">The gateway.</param>
		/// <param name="configurationParameters">The configuration parameters.</param>
		/// <returns></returns>
		public static ConventionBasedDecoratorRegistrationGateway WithMetricsCapturingDecorator<TUniversalMetricsCapturingStrategy>(this ConventionBasedDecoratorRegistrationGateway gateway, MetricsCapturingModuleConfigurationParameters configurationParameters)
			where TUniversalMetricsCapturingStrategy : class, IUniversalMetricsCapturingStrategy
		{
			var gatewayWithMetricsCapturingDecorator = gateway.WithMetricsCapturingDecorator(configurationParameters);
			gatewayWithMetricsCapturingDecorator.RegisterUniversalMetricsCapturingDecorator<TUniversalMetricsCapturingStrategy>(configurationParameters);
			return gatewayWithMetricsCapturingDecorator;
		}

		private static void RegisterMetricsCapturingDecoratorForIndividualQueryHandlerImplementations(this ConventionBasedDecoratorRegistrationGateway gateway, MetricsCapturingModuleConfigurationParameters configurationParameters)
		{
			if (!configurationParameters.QuerySpecificMetricsCapturingDecoratorEnabled)
				return;

			var queryAndResultTypeWithMetricsCapturingStrategyDefinedCollection = new HashSet<QueryAndResultType>(gateway.AssemblyCollection
				.SelectMany(assembly => assembly.GetTypes().Where(t => t.IsMetricsCapturingStrategyForQueryType()))
				.Select(x => x.GetGenericParametersForQueryMetricsCapturingStrategyType())
				.WhereSome());

			bool hasMetricsCapturingStrategyDefinedForQuery(DecoratorPredicateContext c) => c.ToServiceAndImplementationType().HasMetricsCapturingStrategyDefined(queryAndResultTypeWithMetricsCapturingStrategyDefinedCollection);
			gateway.Container.RegisterSingleton(typeof(IMetricsCapturingStrategyForQuery<,>), gateway.AssemblyCollection);
			gateway.Container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(QueryHandlerMetricsCapturingDecorator<,>), gateway.Lifestyle, hasMetricsCapturingStrategyDefinedForQuery);
			gateway.Container.RegisterDecorator(typeof(IAsyncQueryHandler<,>), typeof(AsyncQueryHandlerMetricsCapturingDecorator<,>), gateway.Lifestyle, hasMetricsCapturingStrategyDefinedForQuery);
		}

		private static void RegisterMetricsCapturingDecoratorForIndividualCommandHandlerImplementations(this ConventionBasedDecoratorRegistrationGateway gateway, MetricsCapturingModuleConfigurationParameters configurationParameters)
		{
			if (!configurationParameters.CommandSpecificMetricsCapturingDecoratorEnabled)
				return;

			var commandAndErrorTypeWithMetricsCapturingStrategyDefinedCollection = new HashSet<CommandAndErrorType>(gateway.AssemblyCollection
				.SelectMany(assembly => assembly.GetTypes().Where(t => t.IsMetricsCapturingStrategyForCommandType()))
				.Select(x => x.GetGenericParametersForCommandMetricsCapturingStrategyType())
				.WhereSome());

			bool hasMetricsCapturingStrategyDefinedForCommand(DecoratorPredicateContext c) => c.ToServiceAndImplementationType().HasMetricsCapturingStrategyDefined(commandAndErrorTypeWithMetricsCapturingStrategyDefinedCollection);
			gateway.Container.RegisterSingleton(typeof(IMetricsCapturingStrategyForCommand<,>), gateway.AssemblyCollection);
			gateway.Container.RegisterDecorator(typeof(ICommandHandler<,>), typeof(CommandHandlerMetricsCapturingDecorator<,>), gateway.Lifestyle, hasMetricsCapturingStrategyDefinedForCommand);
			gateway.Container.RegisterDecorator(typeof(IAsyncCommandHandler<,>), typeof(AsyncCommandHandlerMetricsCapturingDecorator<,>), gateway.Lifestyle, hasMetricsCapturingStrategyDefinedForCommand);
		}

		private static void RegisterUniversalMetricsCapturingDecorator<TUniversalMetricsCapturingStrategy>(this ConventionBasedDecoratorRegistrationGateway gateway, MetricsCapturingModuleConfigurationParameters configurationParameters)
			where TUniversalMetricsCapturingStrategy : class, IUniversalMetricsCapturingStrategy
		{
			if (!configurationParameters.UniversalMetricsCapturingDecoratorEnabled)
				return;

			gateway.Container.RegisterSingleton<IUniversalMetricsCapturingStrategy, TUniversalMetricsCapturingStrategy>();
			gateway.Container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(QueryHandlerMetricsCapturingDecoratorForUniversalStrategy<,>), gateway.Lifestyle);
			gateway.Container.RegisterDecorator(typeof(IAsyncQueryHandler<,>), typeof(AsyncQueryHandlerMetricsCapturingDecoratorForUniversalStrategy<,>), gateway.Lifestyle);
			gateway.Container.RegisterDecorator(typeof(ICommandHandler<,>), typeof(CommandHandlerMetricsCapturingDecoratorForUniversalStrategy<,>), gateway.Lifestyle);
			gateway.Container.RegisterDecorator(typeof(IAsyncCommandHandler<,>), typeof(AsyncCommandHandlerMetricsCapturingDecoratorForUniversalStrategy<,>), gateway.Lifestyle);
		}
	}
}