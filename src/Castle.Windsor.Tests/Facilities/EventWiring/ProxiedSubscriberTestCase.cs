﻿// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Tests.Facilities.EventWiring
{
	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.Facilities.EventWiring;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.SubSystems.Configuration;
	using Castle.Windsor;
	using Castle.Windsor.Tests.Facilities.EventWiring.Model;
	using Castle.Windsor.Tests.Interceptors;

	using NUnit.Framework;

	[TestFixture]
	public class ProxiedSubscriberTestCase
	{
		protected WindsorContainer container;
		private IConfigurationStore store;

		[SetUp]
		public void Init()
		{
			container = new WindsorContainer();

			store = (IConfigurationStore) 
				container.Kernel.GetSubSystem(SubSystemConstants.ConfigurationStoreKey);
		}

		[Test]
		public void Wiring_WhenPublisherHasAProxy_ListenerShouldStillGetTheEvents()
		{
			MutableConfiguration config = new MutableConfiguration("component");
			config.
				CreateChild("subscribers").
					CreateChild("subscriber").
					Attribute("id", "listener").
					Attribute("event", "Event").
					Attribute("handler", "OnPublish");
			store.AddComponentConfiguration("publisher", config);

			container.AddFacility("event", new EventWiringFacility());

			// Registers interceptor as component
			container.Register(Component.For<CountingInterceptor>().Named("my.interceptor"));
			
			container.Register(Component.For<SimpleListener>().Named("listener"));
			
			// Publisher has a reference to interceptor
			container.Register(
				Component.For<SimplePublisher>().
					Named("publisher").
					Interceptors(new InterceptorReference("my.interceptor")).First);

			// Triggers events
			SimplePublisher publisher = container.Resolve<SimplePublisher>();
			publisher.Trigger();

			CountingInterceptor interceptor = container.Resolve<CountingInterceptor>();
			Assert.AreEqual(1, interceptor.Intercepted);

			// Assert that event was caught
			SimpleListener listener = container.Resolve<SimpleListener>();
			Assert.IsTrue(listener.Listened);
			Assert.AreSame(publisher, listener.Sender);
		}
	}
}
