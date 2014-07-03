using System;
using System.Collections.Generic;
using System.Reflection;

namespace Gem.DependencyInjector
{
    public class DefinitionMetaData : IComparable<DefinitionMetaData>
    {
        public readonly List<Type> Parameters;
        public readonly MethodInfo ExacutableBody;
        public readonly Type OutPutType;
        public readonly IConfigureDependencies OwningInstance;
        public readonly string Id;

        public DefinitionMetaData(IConfigureDependencies owningInstance, List<Type> parameters, MethodInfo exacutableBody, Type outPutType, string id)
        {
            OwningInstance = owningInstance;
            Parameters = parameters;
            ExacutableBody = exacutableBody;
            OutPutType = outPutType;
            Id = id;
        }

        public int CompareTo(DefinitionMetaData other)
        {
            int otherDependencies = other.Parameters.Count;
            int dependencies = Parameters.Count;
            if (dependencies < otherDependencies) return -1;
            if (dependencies > otherDependencies) return 1;
            return 0;
        }

        public override string ToString()
        {
            return String.Format("{0}. Id:{1}, OutPutType:{2}", base.ToString(), Id, OutPutType);
        }
    }
}