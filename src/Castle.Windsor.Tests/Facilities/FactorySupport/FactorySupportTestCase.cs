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

namespace Castle.Windsor.Tests.Facilities.FactorySupport
{
	using System;
	using System.Collections.Generic;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.Facilities.FactorySupport;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;
	using Castle.MicroKernel.Tests.Lifestyle.Components;

	using NUnit.Framework;

	[TestFixture]
	public class FactorySupportTestCase
	{
		IKernel kernel;

		[SetUp]
		public void SetUp()
		{
			kernel = new DefaultKernel();
		}

		[Test]
		public void NullModelConfigurationBug()
		{
            kernel.AddFacility("factories", new FactorySupportFacility());
            kernel.Register(Component.For<ICustomer>().Named("a").Instance(new CustomerImpl()));
		}

		[Test]
		public void DependancyIgnored()
		{
			kernel.AddFacility("factories", new FactorySupportFacility());
			kernel.Register(Component.For(typeof(Factory)).Named("a"));

			AddComponent("stringdictComponent", typeof(StringDictionaryDependentComponent), "CreateWithStringDictionary");
			AddComponent("hashtableComponent", typeof(HashTableDependentComponent), "CreateWithHashtable");
			AddComponent("serviceComponent", typeof(ServiceDependentComponent), "CreateWithService");

			kernel.Resolve("hashtableComponent", typeof(HashTableDependentComponent));
			kernel.Resolve("serviceComponent", typeof(ServiceDependentComponent));
			kernel.Resolve("stringdictComponent", typeof(StringDictionaryDependentComponent));
		}

		[Test, Ignore("Bug confirmed, but cant fix it without undesired side effects")]
		public void KernelDoesNotTryToWireComponentsPropertiesWithFactoryConfiguration()
		{
			kernel.AddFacility("factories", new FactorySupportFacility());
			kernel.Register(Component.For(typeof(Factory)).Named("a"));

			ComponentModel model = AddComponent("cool.service", typeof(MyCoolServiceWithProperties), "CreateCoolService");

			model.Parameters.Add("someProperty", "Abc");

			var service = kernel.Resolve<MyCoolServiceWithProperties>("cool.service");

			Assert.IsNotNull(service);
			Assert.IsNull(service.SomeProperty);
		}

		[Test,Ignore("Since the facility is mostly for legacy stuff, I don't think it's crucial to support this.") ]
		public void Late_bound_factory_properly_applies_lifetime_concenrns()
		{
			kernel.AddFacility("factories", new FactorySupportFacility());
			kernel.Register(Component.For(typeof(DisposableComponentFactory)).Named("a"));
			var componentModel = AddComponent("foo", typeof (IComponent), "Create");
			componentModel.LifestyleType = LifestyleType.Transient;
			var component = kernel.Resolve<IComponent>("foo") as ComponentWithDispose;
			Assert.IsNotNull(component);
			Assert.IsFalse(component.Disposed);
			kernel.ReleaseComponent(component);
			Assert.IsTrue(component.Disposed);
		}

		private ComponentModel AddComponent(string key, Type type, string factoryMethod)
		{
			var config = new MutableConfiguration(key);
			config.Attributes["factoryId"] = "a";
			config.Attributes["factoryCreate"] = factoryMethod;
			kernel.ConfigurationStore.AddComponentConfiguration(key, config);
			kernel.Register(Component.For(type).Named(key));
			return kernel.GetHandler(key).ComponentModel;
		}

		public class Factory
		{
			public static HashTableDependentComponent CreateWithHashtable()
			{
				return new HashTableDependentComponent(null);
			}

			public static StringDictionaryDependentComponent CreateWithStringDictionary()
			{
				return new StringDictionaryDependentComponent(null);
			}

			public static ServiceDependentComponent CreateWithService()
			{
				return new ServiceDependentComponent(null);
			}

			public static MyCoolServiceWithProperties CreateCoolService(string someProperty)
			{
				return new MyCoolServiceWithProperties();
			}
		}

		public class MyCoolServiceWithProperties
		{
			private string someProperty;

			public string SomeProperty
			{
				get { return someProperty; }
				set { someProperty = value; }
			}
		}

		public class StringDictionaryDependentComponent
		{
			public StringDictionaryDependentComponent(Dictionary<string, object> d)
			{
			}
		}

		public class ServiceDependentComponent
		{
			public ServiceDependentComponent(ICommon d)
			{
			}
		}

		public class HashTableDependentComponent
		{
			public HashTableDependentComponent(Dictionary<object, object> d)
			{
			}
		}
	}

	public class DisposableComponentFactory
	{
		public IComponent Create()
		{
			return new ComponentWithDispose();

		}
	}
}
