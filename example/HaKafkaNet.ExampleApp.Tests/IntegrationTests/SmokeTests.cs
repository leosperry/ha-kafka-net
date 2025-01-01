using System;
using System.Net.Http.Json;
using HaKafkaNet.ExampleApp.Models;
using HaKafkaNet.Testing;
using Moq;

namespace HaKafkaNet.ExampleApp.Tests.IntegrationTests;

public class SmokeTests : IClassFixture<HaKafkaNetFixture>
{
    private readonly HaKafkaNetFixture _fixture;
    private readonly TestHelper _testHelper;


    public SmokeTests(HaKafkaNetFixture fixture)
    {
        this._fixture = fixture;
        this._testHelper = fixture.Helpers;
    }
    
    [Fact]
    public async Task Simple()
    {
        // arrange 
        _fixture.API.Reset();
        var motionState = _testHelper.Make<OnOff>(Binary_Sensor.MotionForSimple, OnOff.On, _testHelper.Time.GetLocalNow());

        // act
        await _testHelper.SendState(motionState);

        // assert
        _fixture.API.Verify(api => api.ButtonPress(Input_Button.HelperButtonForSimple, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task SimpleTyped()
    {
        // arrange 
        _fixture.API.Reset();
        var motionState = _testHelper.Make<OnOff>(Binary_Sensor.MotionForSimpleTyped, OnOff.On, _testHelper.Time.GetLocalNow());

        // act
        await _testHelper.SendState(motionState);

        // assert
        _fixture.API.Verify(api => api.ButtonPress(Input_Button.HelperButtonForSimpleTyped, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task Conditional()
    {
        // arrange 
        _fixture.API.Reset();
        var motionState = _testHelper.Make<OnOff>(Binary_Sensor.MotionForConditional, OnOff.On, _testHelper.Time.GetLocalNow());

        // act
        await _testHelper.SendState(motionState);
        await _testHelper.AdvanceTime(TimeSpan.FromMinutes(61));

        // assert
        _fixture.API.Verify(api => api.ButtonPress(Input_Button.HelperButtonForConditional, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task ConditionalTyped()
    {
        // arrange 

        // clears setup and invocations
        _fixture.API.Reset();
        var motionState = _testHelper.Make<OnOff>(Binary_Sensor.MotionForConditionalTyped, OnOff.On, _testHelper.Time.GetLocalNow());

        // act
        await _testHelper.SendState(motionState);
        await _testHelper.AdvanceTime(TimeSpan.FromMinutes(61));

        // assert
        _fixture.API.Verify(api => api.ButtonPress(Input_Button.HelperButtonForConditionalTyped, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task Schedulable()
    {
        // arrange 

        // clears setup and invocations
        _fixture.API.Reset();
        var motionState = _testHelper.Make<OnOff>(Binary_Sensor.MotionForSchedulable, OnOff.On);

        // act
        await _testHelper.SendState(motionState);
        await _testHelper.AdvanceTime(TimeSpan.FromMinutes(61));

        // assert
        _fixture.API.Verify(api => api.ButtonPress(Input_Button.HelperButtonForSchedulable, It.IsAny<CancellationToken>()));
    }

[Fact]
public async Task SchedulableTyped()
{
    // arrange 

    _fixture.API.Reset();
    var motionState = _testHelper.Make<OnOff>(
        Binary_Sensor.MotionForSchedulableTyped, OnOff.On);

    // act
    await _testHelper.SendState(motionState);
    await _testHelper.AdvanceTime(TimeSpan.FromMinutes(61));

    // assert
    _fixture.API.Verify(api => api.ButtonPress(
        Input_Button.HelperButtonForSchedulableTyped, It.IsAny<CancellationToken>()));
}

    [Fact]
    public async Task VerifyMetaData()
    {
        var client = _fixture.CreateClient();

        var details  = await client.GetFromJsonAsync<ApiResponse<AutomationListResponse>>("api/automations");

        var automationNames = 
            new string[]{
                nameof(Simple), 
                nameof(SimpleTyped),
                nameof(Conditional),
                nameof(ConditionalTyped),
                nameof(Schedulable),
                nameof(SchedulableTyped),
                
            }.Select(s => $"{nameof(TestRegistry)}.{s}");

        var hashedNames = details!.Data.Automations.Select(a => a.Name).ToHashSet();

        foreach (var auto in automationNames)
        {
            Assert.Contains(auto, hashedNames);
        }
    }

    [Fact]
    public async Task LongDelay()
    {
        // arrange 

        _fixture.API.Reset();
        var motionState = _testHelper.Make<OnOff>(
            Binary_Sensor.TriggerForLongDelay, OnOff.On, _testHelper.Time.GetLocalNow());

        // act
        await _testHelper.SendState(motionState);
        await _testHelper.AdvanceTime(TimeSpan.FromDays(51));

        // assert
        _fixture.API.Verify(api => api.ButtonPress(
            Input_Button.HelperButtonForLongDelay, It.IsAny<CancellationToken>()));
    }    
    
    [Fact]
    public async Task LongDelay_NegativeCase()
    {
        // arrange 

        _fixture.API.Reset();
        var motionState = _testHelper.Make<OnOff>(
            Binary_Sensor.TriggerForLongDelay, OnOff.On, _testHelper.Time.GetLocalNow());

        // act
        await _testHelper.SendState(motionState);
        await _testHelper.AdvanceTime(TimeSpan.FromDays(49));

        // assert
        _fixture.API.Verify(api => api.ButtonPress(
            Input_Button.HelperButtonForLongDelay, It.IsAny<CancellationToken>()), Times.Never);
    }
}
