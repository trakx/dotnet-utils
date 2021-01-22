using System.Linq;
using FluentAssertions;
using Trakx.Utils.Extensions;
using Xunit;

namespace Trakx.Utils.Tests.Unit.Extensions
{
    public class EnumerableExtensionsTests
    {
        private class ObjectToCompare
        {
            public string? Name { get; init; }
            public string? Reference { get; init; }
        }
       
        [Fact]
        public void DistinctBy_should_compare_items_using_selector()
        {
            var classes = new[]
            {
                new ObjectToCompare {Name = "abc", Reference = "123"},
                new ObjectToCompare {Name = "def", Reference = "123"},
                new ObjectToCompare {Name = "ghi", Reference = "123"},
                new ObjectToCompare {Name = "abc", Reference = "456"},
            };

            classes.DistinctBy(c => c!.Name).Should().BeEquivalentTo(classes.Take(3));
            classes.DistinctBy(c => c!.Reference).Should().BeEquivalentTo(classes[0]!, classes[3]!);
        }
    }
}