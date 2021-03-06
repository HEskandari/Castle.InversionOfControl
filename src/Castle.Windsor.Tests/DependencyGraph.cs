// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.MicroKernel.Tests
{
	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.Windsor;
	using Castle.Windsor.Tests;
	using Castle.Windsor.Tests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class DependencyGraph
	{
		private IKernel kernel;

		[SetUp]
		public void Init()
		{
			kernel = new DefaultKernel();
		}

		[TearDown]
		public void Dispose()
		{
			kernel.Dispose();
		}

		[Test]
		public void ValidSituation()
		{
			kernel.Register(Component.For<A>(),
			                Component.For<B>(),
			                Component.For<C>());

			Assert.IsNotNull(kernel.Resolve<A>());
			Assert.IsNotNull(kernel.Resolve<B>());
			Assert.IsNotNull(kernel.Resolve<C>());
		}

		[Test]
		public void GraphInvalid()
		{
			kernel.Register(Component.For(typeof(B)).Named("b"));
			kernel.Register(Component.For(typeof(C)).Named("c"));

			IHandler handlerB = kernel.GetHandler(typeof(B));
			IHandler handlerC = kernel.GetHandler(typeof(C));

			Assert.AreEqual(HandlerState.WaitingDependency, handlerB.CurrentState);
			Assert.AreEqual(HandlerState.WaitingDependency, handlerC.CurrentState);
		}

		[Test]
		public void GraphInvalidAndLateValidation()
		{
			kernel.Register(Component.For(typeof(B)).Named("b"));
			kernel.Register(Component.For(typeof(C)).Named("c"));

			IHandler handlerB = kernel.GetHandler(typeof(B));
			IHandler handlerC = kernel.GetHandler(typeof(C));

			Assert.AreEqual(HandlerState.WaitingDependency, handlerB.CurrentState);
			Assert.AreEqual(HandlerState.WaitingDependency, handlerC.CurrentState);

			kernel.Register(Component.For(typeof(A)).Named("a"));

			Assert.AreEqual(HandlerState.Valid, handlerB.CurrentState);
			Assert.AreEqual(HandlerState.Valid, handlerC.CurrentState);
		}

		[Test]
		public void CycleComponentGraphs()
		{
			kernel.Register(Component.For(typeof(CycleA)).Named("a"));
			kernel.Register(Component.For(typeof(CycleB)).Named("b"));

			var exception =
				Assert.Throws(typeof(HandlerException), () =>
				{
					var a = kernel.Resolve<CycleA>("a");
				});
			string expectedMessage =
				"Can't create component 'a' as it has dependencies to be satisfied. \r\n" +
				"a is waiting for the following dependencies: \r\n\r\n" +
				"Services: \r\n" +
				"- Castle.Windsor.Tests.CycleB which was registered but is also waiting for dependencies. \r\n\r\n" +

				"b is waiting for the following dependencies: \r\n\r\n" +

				"Services: \r\n" +
				"- Castle.Windsor.Tests.CycleA which was registered but is also waiting for dependencies. \r\n";
			Assert.AreEqual(expectedMessage, exception.Message);
		}
	}
}
