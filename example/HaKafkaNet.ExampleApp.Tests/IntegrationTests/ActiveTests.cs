// using System;
// using HaKafkaNet.Testing;
// using Moq;

// namespace HaKafkaNet.ExampleApp.Tests.IntegrationTests;

// public class ActiveTests : IClassFixture<HaKafkaNetFixture>
// {
//     private readonly HaKafkaNetFixture _fixture;
//     private readonly TestHelper _testHelper;

//     public ActiveTests(HaKafkaNetFixture fixture)
//     {
//         this._fixture = fixture;
//         this._testHelper = fixture.Helpers;
//     }
    
//     // this test seems to hang the test runner when run via git actions

//     [Fact]
//     public Task ActiveFiresOnStartup()
//     {
//         _fixture.API.Verify(api => api.ButtonPress("my.button", default));
//         return Task.CompletedTask;
//     }
// }
