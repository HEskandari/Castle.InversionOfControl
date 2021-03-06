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

namespace Castle.MicroKernel.Registration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	
	/// <summary>
	/// Selects a set of types from an assembly.
	/// </summary>
	public class FromAssemblyDescriptor : FromDescriptor
	{
		private readonly IEnumerable<Assembly> assemblies;
		private bool nonPublicTypes;

		internal FromAssemblyDescriptor(Assembly assembly)
		{
			assemblies = new[] { assembly };
		}

		internal FromAssemblyDescriptor(IEnumerable<Assembly> assemblies)
		{
			this.assemblies = assemblies;
		}

		public FromAssemblyDescriptor IncludeNonPublicTypes()
		{
			nonPublicTypes = true;
			return this;
		}

		protected override IEnumerable<Type> SelectedTypes(IKernel kernel)
		{
			if (nonPublicTypes)
			{
				foreach (var type in assemblies.SelectMany(assembly => assembly.GetTypes()))
				{
					yield return type;
				}
			}

			foreach (var type in assemblies.SelectMany(assembly => assembly.GetExportedTypes()))
			{
				yield return type;
			}
		}
	}
}
