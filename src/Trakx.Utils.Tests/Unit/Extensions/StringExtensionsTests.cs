﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Trakx.Utils.Tests.Unit.Extensions
{
    public class StringExtensionsTests
    {
        private readonly ITestOutputHelper _output;

        public StringExtensionsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SplitCsvToDistinctList_should_work_on_empty_strings()
        {
            " ".SplitCsvToLowerCaseDistinctList().Should().NotBeNull().And.BeEmpty();
            "".SplitCsvToLowerCaseDistinctList().Should().NotBeNull().And.BeEmpty();
            "\t".SplitCsvToLowerCaseDistinctList().Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void SplitCsvToDistinctList_should_trim_and_lowercase_records()
        {
            "\tabc, dEf, ghi,HELLO ".SplitCsvToLowerCaseDistinctList().Should()
                .BeEquivalentTo(new List<string> {"abc", "def", "ghi", "hello"});
        }

        [Fact]
        public void SplitCsvToDistinctList_should_remove_duplicates()
        {
            "\tabc, def, ghi,GHI,Abc,hello ".SplitCsvToLowerCaseDistinctList().Should()
                .BeEquivalentTo(new List<string> { "abc", "def", "ghi", "hello" });
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("a", "A")]
        [InlineData("A", "A")]
        [InlineData("1", "1")]
        [InlineData("_a", "_a")]
        [InlineData("AbC", "AbC")]
        [InlineData("def", "Def")]
        [InlineData("1bc", "1bc")]
        public void FirstCharToUpperCase_should_not_error(string input, string expectedOutput)
        {
            input.FirstCharToUpper().Should().Be(expectedOutput);
        }

        [Fact]
        public void ToCronSchedule_should_parse_string_and_return_observable_schedule_times_which_are_cancellable()
        {
            var scheduler = new TestScheduler();
            using var cancellationSource = new CancellationTokenSource();
            var end = TimeSpan.FromMinutes(60).Add(TimeSpan.FromTicks(1));
            const string cron = "*/1 * * * *"; //once a minute
            var triggeredEvents = new List<DateTimeOffset>();
            using var sub = cron.ToCronObservable(cancellationSource.Token, scheduler)
                .Subscribe(t =>
                {
                    _output.WriteLine($"OnNext received at {scheduler.Now} with timestamp {t}");
                    triggeredEvents.Add(t);
                });

            scheduler.Schedule(end, () => scheduler.Stop());
            scheduler.Start();

            triggeredEvents.Count.Should().Be(60);
            triggeredEvents.Should().BeEquivalentTo(Enumerable.Range(1, 60)
                .Select(i => new DateTimeOffset().Add(TimeSpan.FromMinutes(i))));
        }
    }
}