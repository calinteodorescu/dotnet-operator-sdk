// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;

using FluentAssertions;

using KubeOps.Generator.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace KubeOps.Generator.Test;

public sealed class ControllerRegistrationGeneratorTest
{
    [Theory]
    [InlineData("", """
                    // <auto-generated>
                    // This code was generated by a tool.
                    // Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
                    // </auto-generated>
                    #pragma warning disable CS1591
                    using KubeOps.Abstractions.Builder;

                    public static class ControllerRegistrations
                    {
                        public static IOperatorBuilder RegisterControllers(this IOperatorBuilder builder)
                        {
                            return builder;
                        }
                    }
                    """)]
    [InlineData("""
                using k8s;
                using k8s.Models;
                using KubeOps.Abstractions.Controller;
                
                [KubernetesEntity(Group = "testing.dev", ApiVersion = "v1", Kind = "TestEntity")]
                public class V1TestEntity : IKubernetesObject<V1ObjectMeta>
                {
                }

                public class V1TestEntityController : IEntityController<V1TestEntity>
                {
                }
                """, """
                     // <auto-generated>
                     // This code was generated by a tool.
                     // Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
                     // </auto-generated>
                     #pragma warning disable CS1591
                     using KubeOps.Abstractions.Builder;

                     public static class ControllerRegistrations
                     {
                         public static IOperatorBuilder RegisterControllers(this IOperatorBuilder builder)
                         {
                             builder.AddController<global::V1TestEntityController, global::V1TestEntity>();
                             return builder;
                         }
                     }
                     """)]
    [InlineData("""
                using k8s;
                using k8s.Models;
                using KubeOps.Abstractions.Controller;

                [KubernetesEntity(Group = "testing.dev", ApiVersion = "v1", Kind = "TestEntity")]
                public sealed class V1TestEntity : IKubernetesObject<V1ObjectMeta>
                {
                }

                public abstract class EntityControllerBase<TEntity> : IEntityController<TEntity>
                    where TEntity : IKubernetesObject<V1ObjectMeta>
                {
                }
                
                public sealed class V1TestEntityController : EntityControllerBase<V1TestEntity>
                {
                }
                """, """
                     // <auto-generated>
                     // This code was generated by a tool.
                     // Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
                     // </auto-generated>
                     #pragma warning disable CS1591
                     using KubeOps.Abstractions.Builder;

                     public static class ControllerRegistrations
                     {
                         public static IOperatorBuilder RegisterControllers(this IOperatorBuilder builder)
                         {
                             builder.AddController<global::V1TestEntityController, global::V1TestEntity>();
                             return builder;
                         }
                     }
                     """)]
    public void Should_Generate_Correct_Code(string input, string expectedResult)
    {
        var inputCompilation = input.CreateCompilation();
        expectedResult = expectedResult.ReplaceLineEndings();

        var driver = CSharpGeneratorDriver.Create(new ControllerRegistrationGenerator());
        driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var output, out ImmutableArray<Diagnostic> _);

        var result = output.SyntaxTrees
            .First(s => s.FilePath.Contains("ControllerRegistrations.g.cs"))
            .ToString().ReplaceLineEndings();
        result.Should().Be(expectedResult);
    }
}
